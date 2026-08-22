using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using AppDimens.Net.Common;

namespace AppDimens.Net.Core;

/// <summary>Thread-local metrics scope so nested strategy calls inherit the enclosing snapshot.</summary>
public static class MetricsScopeHolder
{
    [ThreadStatic]
    private static DimenMetrics? _current;

    public static DimenMetrics? Current => _current;
    public static void Set(DimenMetrics? value) => _current = value;
}

/// <summary>
/// Global, lock-free, shared cache for all AppDimens dimension calculations.
/// Partitioned per immutable window snapshot (<see cref="DimenMetrics"/>); entries are
/// published as single atomic references, so concurrent readers can never observe
/// another key's value. Bit-identical semantics to the Kotlin/Android originals.
/// </summary>
public static class DimenCache
{
    public const float InvBaseRatio = 0.0033333334f;      // 1f / 300f
    public const float AdjustmentScale = 0.10f / 30f;     // 0.0033333334f
    public const float SensitivityDefault = 0.08f / 30f;  // 0.0026666667f

    public enum CalcType
    {
        Auto, Diagonal, Fill, Fit, Fluid, Interpolated, Logarithmic,
        Percent, Perimeter, Power, Resize, Scaled, Unities, AspectRatio, Density,
    }

    internal const int CtPercent = (int)CalcType.Percent;
    internal const int CtScaled = (int)CalcType.Scaled;
    internal const int CtDensity = (int)CalcType.Density;
    internal const int CtAspectRatio = (int)CalcType.AspectRatio;
    internal const int CtDiagonal = (int)CalcType.Diagonal;
    internal const int CtInterpolated = (int)CalcType.Interpolated;
    internal const int CtPerimeter = (int)CalcType.Perimeter;
    internal const int CtPower = (int)CalcType.Power;
    internal const int CtLogarithmic = (int)CalcType.Logarithmic;

    public enum ValueType
    {
        Dp, Px, SpWithScale, SpNoScale, SpPxWithScale, SpPxNoScale,
    }

    private static volatile bool _isEnabled = true;
    public static bool CacheEnabled { get => _isEnabled; set => _isEnabled = value; }

    private static volatile bool _diagnosticsEnabled;
    public static bool DiagnosticsEnabled { get => _diagnosticsEnabled; set => _diagnosticsEnabled = value; }
    public static long HitCount => Interlocked.Read(ref _hitCount);
    public static long MissCount => Interlocked.Read(ref _missCount);
    public static long EvictionCount => Interlocked.Read(ref _evictionCount);
    private static long _hitCount, _missCount, _evictionCount;

    // ─────────────────────────────────────────────────────────────────────
    // SNAPSHOT-PARTITIONED CACHE — 4 snapshots x 512 entries
    // ─────────────────────────────────────────────────────────────────────

    private const int MaxSnapshotCaches = 4;
    private const int SnapshotCacheSize = 2048 / MaxSnapshotCaches;
    private const int SnapshotCacheMask = SnapshotCacheSize - 1;

    internal sealed class CacheEntry(long key, int valueBits)
    {
        public readonly long Key = key;
        public readonly int ValueBits = valueBits;
    }

    internal sealed class SnapshotCache(int size)
    {
        public readonly CacheEntry[] Entries = new CacheEntry[size];
    }

    private static readonly object SnapshotLock = new();
    private static readonly Dictionary<DimenMetrics, SnapshotCache> SnapshotCaches = new();

    internal static readonly CacheEntry EmptyEntry = new(0L, int.MinValue);

    /// <summary>One volatile holder pairing window context with its metrics.</summary>
    internal sealed class FastWindowSlot(IAppDimensContext? context, DimenMetrics metrics)
    {
        public readonly IAppDimensContext? Context = context;
        public readonly DimenMetrics Metrics = metrics;
    }

    private static readonly FastWindowSlot EmptyFastWindowSlot = new(null, DimenMetrics.DefaultInstance);
    private static FastWindowSlot _fastWindowSlot = EmptyFastWindowSlot;

    internal sealed class FastPartitionSlot(DimenMetrics metrics, SnapshotCache partition)
    {
        public readonly DimenMetrics Metrics = metrics;
        public readonly SnapshotCache Partition = partition;
    }

    internal static readonly SnapshotCache EmptyPartition = new(0);
    private static readonly FastPartitionSlot EmptyFastPartitionSlot =
        new(DimenMetrics.DefaultInstance, EmptyPartition);
    private static FastPartitionSlot _fastPartitionSlot = EmptyFastPartitionSlot;

    private sealed class MwSlot(IAppDimensContext? context, bool mode)
    {
        public readonly IAppDimensContext? Context = context;
        public readonly bool Mode = mode;
    }

    private static readonly MwSlot EmptyMwSlot = new(null, false);
    private static MwSlot _fastMwSlot = EmptyMwSlot;

    private static readonly object MetricsByContextLock = new();
    private static readonly ConditionalWeakTable<IAppDimensContext, DimenMetrics> MetricsByContext = new();

    private static DimenMetrics _fallbackMetrics = DimenMetrics.DefaultInstance;
    public static DimenMetrics FallbackMetrics { get => Volatile.Read(ref _fallbackMetrics); set => Volatile.Write(ref _fallbackMetrics, value); }

    public static DimenMetrics CurrentMetrics =>
        MetricsScopeHolder.Current ?? FallbackMetrics;

    public static float CurrentNormalizedAr => CurrentMetrics.NormalizedAspectRatio;
    public static float CurrentLogNormalizedAr => CurrentMetrics.LogNormalizedAspectRatio;
    public static int CurrentSmallestWidthDp => (int)CurrentMetrics.SmallestWidthDp;
    public static float CurrentDensity => CurrentMetrics.Density;
    public static float CurrentScale => CurrentMetrics.Scale;
    public static float CurrentArMultiplier => CurrentMetrics.DefaultScaledAspectRatioMultiplier;
    public static float CurrentAspectRatioMul => CurrentMetrics.DefaultAspectRatioMultiplier;

    // ─────────────────────────────────────────────────────────────────────
    // EVENT-DRIVEN CONFIG WATCHER
    // ─────────────────────────────────────────────────────────────────────

    private sealed class WatcherEntry(IDisposable registration)
    {
        public IDisposable Registration = registration;
        public int Consumers;
    }

    private static readonly ConditionalWeakTable<IAppDimensContext, WatcherEntry> WatchedContexts = new();
    private static readonly object WatcherLock = new();

    private static void EnsureConfigWatcher(IAppDimensContext context)
    {
        lock (WatcherLock)
        {
            if (!WatchedContexts.TryGetValue(context, out var entry))
            {
                var registration = context.RegisterConfigurationListener(OnContextConfigChanged);
                entry = new WatcherEntry(registration) { Consumers = 0 };
                WatchedContexts.AddOrUpdate(context, entry);
            }
        }
    }

    public static void AcquireConfigWatcher(IAppDimensContext context)
    {
        lock (WatcherLock)
        {
            if (WatchedContexts.TryGetValue(context, out var entry))
            {
                entry.Consumers++;
            }
            else
            {
                var registration = context.RegisterConfigurationListener(OnContextConfigChanged);
                WatchedContexts.AddOrUpdate(context, new WatcherEntry(registration) { Consumers = 1 });
            }
        }
    }

    public static void ReleaseConfigWatcher(IAppDimensContext context)
    {
        lock (WatcherLock)
        {
            if (!WatchedContexts.TryGetValue(context, out var entry)) return;
            if (--entry.Consumers <= 0)
            {
                entry.Registration.Dispose();
                WatchedContexts.Remove(context);
            }
        }
    }

    public static void DisposeConfigWatcher(IAppDimensContext context)
    {
        lock (WatcherLock)
        {
            if (WatchedContexts.TryGetValue(context, out var entry))
            {
                entry.Registration.Dispose();
                WatchedContexts.Remove(context);
            }
        }
    }

    private static void OnContextConfigChanged()
    {
        Volatile.Write(ref _fastWindowSlot, EmptyFastWindowSlot);
        Volatile.Write(ref _fastMwSlot, EmptyMwSlot);
    }

    private static bool MwModeFor(IAppDimensContext? context)
    {
        var slot = Volatile.Read(ref _fastMwSlot);
        if (ReferenceEquals(slot.Context, context)) return slot.Mode;
        var rebuilt = context?.IsInMultiWindowMode == true;
        Volatile.Write(ref _fastMwSlot, new MwSlot(context, rebuilt));
        return rebuilt;
    }

    private static bool FastMatch(DimenMetrics m, ScreenConfiguration c, bool isMultiWindow) =>
        m.ScreenWidthDp == c.ScreenWidthDp &&
        m.ScreenHeightDp == c.ScreenHeightDp &&
        m.SmallestScreenWidthDp == c.SmallestScreenWidthDp &&
        m.DensityDpi == c.DensityDpi &&
        m.FontScaleBits == BitConverter.SingleToInt32Bits(c.FontScale) &&
        m.Orientation == c.Orientation &&
        m.UiMode == c.UiMode &&
        m.IsInMultiWindowMode == isMultiWindow;

    internal static DimenMetrics MetricsFor(IAppDimensContext? context)
    {
        if (context is null) return FallbackMetrics;
        EnsureConfigWatcher(context);

        var fast = Volatile.Read(ref _fastWindowSlot);
        if (!ReferenceEquals(fast, EmptyFastWindowSlot) && ReferenceEquals(fast.Context, context))
        {
            var cfg = context.Configuration;
            if (FastMatch(fast.Metrics, cfg, MwModeFor(context))) return fast.Metrics;
        }

        lock (MetricsByContextLock)
        {
            if (MetricsByContext.TryGetValue(context, out var cached))
            {
                var cfg = context.Configuration;
                if (FastMatch(cached, cfg, MwModeFor(context)))
                {
                    Volatile.Write(ref _fastWindowSlot, new FastWindowSlot(context, cached));
                    return cached;
                }
            }
            var rebuilt = DimenMetrics.From(context.Configuration, context.IsInMultiWindowMode);
            MetricsByContext.AddOrUpdate(context, rebuilt);
            Volatile.Write(ref _fastWindowSlot, new FastWindowSlot(context, rebuilt));
            return rebuilt;
        }
    }

    // Single-window fast memo for the partition lookup: metrics + partition published
    // as ONE coherent atomic state (fixes the KMP 1.0.1 race where a reader could pair
    // metrics(A) with partition(B)).
    internal static FastPartitionSlot CacheFor(DimenMetrics metrics)
    {
        lock (SnapshotLock)
        {
            if (!SnapshotCaches.TryGetValue(metrics, out var cache))
            {
                if (SnapshotCaches.Count >= MaxSnapshotCaches)
                {
                    foreach (var k in SnapshotCaches.Keys)
                    {
                        if (!ReferenceEquals(k, metrics))
                        {
                            SnapshotCaches.Remove(k);
                            break;
                        }
                    }
                }
                cache = new SnapshotCache(SnapshotCacheSize);
                for (var i = 0; i < SnapshotCacheSize; i++) cache.Entries[i] = EmptyEntry;
                SnapshotCaches[metrics] = cache;
            }
            return new FastPartitionSlot(metrics, cache);
        }
    }

    internal static int SlotFor(long key)
    {
        var h = (int)(key ^ (long)((ulong)key >> 32));
        h ^= h >> 16;
        return h & SnapshotCacheMask;
    }

    public static DimenMetrics MetricsCoherentFor(IAppDimensContext? context)
    {
        var scope = MetricsScopeHolder.Current;
        if (scope is not null) return scope;
        var slot = Volatile.Read(ref _fastWindowSlot);
        if (!ReferenceEquals(slot, EmptyFastWindowSlot) && ReferenceEquals(slot.Context, context))
            return slot.Metrics;
        return MetricsFor(context);
    }

    public static DimenMetrics FastMetricsForCode(IAppDimensContext? context)
    {
        var slot = Volatile.Read(ref _fastWindowSlot);
        if (!ReferenceEquals(slot, EmptyFastWindowSlot) && ReferenceEquals(slot.Context, context))
            return slot.Metrics;
        return MetricsFor(context);
    }

    /// <summary>True when the key must always compute directly (cache off, custom-K or cheap multiply bypass).</summary>
    public static bool ShouldComputeDirectly(long key) =>
        !_isEnabled || HasCustomSensitivityKey(key) || ShouldBypassCache(key);

    /// <summary>Allocation-free hit probe; false for disabled cache, custom-K and bypass keys.</summary>
    public static bool TryPeek(DimenMetrics metrics, long key, out float value)
    {
        value = 0f;
        if (!_isEnabled || HasCustomSensitivityKey(key) || ShouldBypassCache(key)) return false;
        var slot = Volatile.Read(ref _fastPartitionSlot);
        if (!slot.Metrics.Equals(metrics)) return false;
        var e = slot.Partition.Entries[SlotFor(key)];
        if (ReferenceEquals(e, EmptyEntry) || e.Key != key) return false;
        value = BitConverter.Int32BitsToSingle(e.ValueBits);
        return true;
    }

    /// <summary>Core resolution — zero allocation on the hit path.</summary>
    public static float Resolve(long key, DimenMetrics metrics, Func<float> compute)
    {
        if (!_isEnabled || HasCustomSensitivityKey(key) || ShouldBypassCache(key))
            return WithMetrics(metrics, compute);

        var slot = Volatile.Read(ref _fastPartitionSlot);
        if (ReferenceEquals(slot.Partition, EmptyPartition) || !slot.Metrics.Equals(metrics))
        {
            slot = CacheFor(metrics);
            Volatile.Write(ref _fastPartitionSlot, slot);
        }
        var partition = slot.Partition;
        var s = SlotFor(key);
        var existing = partition.Entries[s];
        if (!ReferenceEquals(existing, EmptyEntry) && existing.Key == key)
        {
            if (_diagnosticsEnabled) Interlocked.Increment(ref _hitCount);
            return BitConverter.Int32BitsToSingle(existing.ValueBits);
        }
        if (_diagnosticsEnabled) Interlocked.Increment(ref _missCount);
        var computed = WithMetrics(metrics, compute);
        if (!float.IsFinite(computed)) return computed;
        if (_diagnosticsEnabled && !ReferenceEquals(existing, EmptyEntry))
            Interlocked.Increment(ref _evictionCount);
        Volatile.Write(ref partition.Entries[s], new CacheEntry(key, BitConverter.SingleToInt32Bits(computed)));
        return computed;
    }

    internal static T WithMetrics<T>(DimenMetrics metrics, Func<T> block)
    {
        var previous = MetricsScopeHolder.Current;
        if (ReferenceEquals(previous, metrics)) return block();
        MetricsScopeHolder.Set(metrics);
        try { return block(); }
        finally { MetricsScopeHolder.Set(previous); }
    }

    public static void WithCompositionMetrics(DimenMetrics? metrics, Action block)
    {
        if (metrics is null) { block(); return; }
        WithMetrics<object?>(metrics, () => { block(); return null; });
    }

    // ─────────────────────────────────────────────────────────────────────
    // SPECIALIZED KERNELS — one kernel per family/qualifier, zero branches.
    // PX lanes keep BOTH multiplies (base * factor * density); DP lanes omit
    // density (IEEE-754 is not associative — pre-combining changes rounding).
    // ─────────────────────────────────────────────────────────────────────

    public static float ResolveSdpPx(float baseValue, IAppDimensContext? context)
    {
        var m = FastMetricsForCode(context);
        return baseValue * m.Scale * m.Density;
    }

    public static float ResolveSdpDp(float baseValue, IAppDimensContext? context)
    {
        var m = FastMetricsForCode(context);
        // Multiply in the exact kernel order (base × dim × ratio) so results are
        // bitwise identical to ToDynamicScaledDp — associativity changes rounding.
        return baseValue * m.SmallestWidthDp * InvBaseRatio;
    }

    public static float ResolveSdpaPx(float baseValue, IAppDimensContext? context)
    {
        var m = FastMetricsForCode(context);
        return baseValue * m.DefaultScaledAspectRatioMultiplier * m.Density;
    }

    public static float ResolveSdpaDp(float baseValue, IAppDimensContext? context)
    {
        var m = FastMetricsForCode(context);
        return baseValue * m.DefaultScaledAspectRatioMultiplier;
    }

    public static float ResolveHdpPx(float baseValue, IAppDimensContext? context)
    {
        var m = FastMetricsForCode(context);
        return baseValue * m.ScreenHeightDp * InvBaseRatio * m.Density;
    }

    public static float ResolveHdpDp(float baseValue, IAppDimensContext? context)
    {
        var m = FastMetricsForCode(context);
        return baseValue * m.ScreenHeightDp * InvBaseRatio;
    }

    public static float ResolveWdpPx(float baseValue, IAppDimensContext? context)
    {
        var m = FastMetricsForCode(context);
        return baseValue * m.ScreenWidthDp * InvBaseRatio * m.Density;
    }

    public static float ResolveWdpDp(float baseValue, IAppDimensContext? context)
    {
        var m = FastMetricsForCode(context);
        return baseValue * m.ScreenWidthDp * InvBaseRatio;
    }

    internal static float FastScaledMultiplier(DimenMetrics m, DpQualifier qualifier, bool applyAspectRatio)
    {
        if (qualifier == DpQualifier.SmallWidth)
            return applyAspectRatio ? m.DefaultScaledAspectRatioMultiplier : m.Scale;
        if (qualifier == DpQualifier.Width) return m.ScreenWidthFactor;
        return m.ScreenHeightFactor;
    }

    public static float CalculateRawScaling(float baseValue, bool applyAspectRatio, float? customSensitivityK)
    {
        if (!float.IsFinite(baseValue)) throw new ArgumentException("baseValue must be finite", nameof(baseValue));
        return baseValue * CurrentMetrics.ScaledMultiplier(applyAspectRatio, customSensitivityK);
    }

    public static bool IsScalingEnabled() => _isEnabled;

    public static void Init(IAppDimensContext context)
    {
        EnsureConfigWatcher(context);
        FallbackMetrics = DimenMetrics.From(context.Configuration, context.IsInMultiWindowMode);
    }

    public static void Shutdown() { }

    /// <summary>Compatibility hook: nulls fast slots after any real configuration change.</summary>
    public static void InvalidateOnConfigChange(ScreenConfiguration newConfiguration)
    {
        OnContextConfigChanged();
    }

    public static void AddResetListener(Action listener)
    {
        lock (ResetListenersLock) ResetListeners.Add(listener);
    }

    public static void RemoveResetListener(Action listener)
    {
        lock (ResetListenersLock) ResetListeners.Remove(listener);
    }

    private static void NotifyResetListeners()
    {
        Action[] snapshot;
        lock (ResetListenersLock) snapshot = ResetListeners.ToArray();
        foreach (var l in snapshot) l();
    }

    private static readonly object ResetListenersLock = new();
    private static readonly List<Action> ResetListeners = [];

    public static void ClearAll()
    {
        lock (SnapshotLock) SnapshotCaches.Clear();
        Volatile.Write(ref _fastPartitionSlot, EmptyFastPartitionSlot);
        OnContextConfigChanged();
        NotifyResetListeners();
    }

    // ─────────────────────────────────────────────────────────────────────
    // CACHED UiModeType — fingerprint per window context
    // ─────────────────────────────────────────────────────────────────────

    private sealed class UiModeCacheEntry(int fingerprint, Common.UiModeType value)
    {
        public readonly int Fingerprint = fingerprint;
        public readonly Common.UiModeType Value = value;
    }

    private static volatile Common.UiModeType _cachedUiMode = Common.UiModeType.Undefined;
    private static readonly ConditionalWeakTable<IAppDimensContext, UiModeCacheEntry> UiModeByContext = new();
    private static readonly object UiModeLock = new();

    public static Common.UiModeType GetCachedUiModeType(IAppDimensContext? context)
    {
        if (context is null) return _cachedUiMode;
        var cfg = context.Configuration;
        var fingerprint =
            (cfg.UiMode * 31 + cfg.SmallestScreenWidthDp) * 31 +
            Math.Min(cfg.ScreenWidthDp, cfg.ScreenHeightDp) * 31 +
            Math.Max(cfg.ScreenWidthDp, cfg.ScreenHeightDp);
        lock (UiModeLock)
        {
            if (UiModeByContext.TryGetValue(context, out var cached) && cached.Fingerprint == fingerprint)
                return cached.Value;
            var mode = context.UiModeType;
            UiModeByContext.AddOrUpdate(context, new UiModeCacheEntry(fingerprint, mode));
            _cachedUiMode = mode;
            return mode;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // KEY ENCODING — packed 64-bit
    // [63] AR | [62-31] base bits | [30-27] CalcType | [26-24] ValueType |
    // [23-8] sensitivityK fp16 | [7-6] qualifier | [5-2] inverter | [1] land | [0] imw
    // ─────────────────────────────────────────────────────────────────────

    public static long BuildKey(
        float baseValue, bool isLandscape, bool ignoreMultiWindows,
        CalcType calcType, DpQualifier qualifier, Inverter inverter,
        bool applyAspectRatio, ValueType valueType, float? customSensitivityK = null)
    {
        if (!float.IsFinite(baseValue)) throw new ArgumentException("baseValue must be finite", nameof(baseValue));
        if (customSensitivityK.HasValue && !float.IsFinite(customSensitivityK.Value))
            throw new ArgumentException("customSensitivityK must be finite", nameof(customSensitivityK));

        var ar = applyAspectRatio ? 1L : 0L;
        var bv = BitConverter.SingleToInt32Bits(baseValue) & 0xFFFFFFFFL;
        var ct = (uint)calcType & 0xFL;
        var vt = (uint)valueType & 0x7L;
        var sk = customSensitivityK.HasValue
            ? (BitConverter.SingleToInt32Bits(customSensitivityK.Value) >>> 16) & 0xFFFFL
            : 0xFFFFL;
        var q = ((uint)qualifier & 0x3L);
        var inv = ((uint)inverter & 0xFL);
        var land = calcType is CalcType.Diagonal or CalcType.Perimeter or CalcType.Density
            ? 0L
            : isLandscape ? 1L : 0L;
        var imw = ignoreMultiWindows ? 1L : 0L;

        return (ar << 63) | (bv << 31) | ((long)ct << 27) | ((long)vt << 24) |
               (sk << 8) | (q << 6) | (inv << 2) | (land << 1) | imw;
    }

    public static long BuildKey(
        int baseValue, bool isLandscape, bool ignoreMultiWindows,
        CalcType calcType, DpQualifier qualifier, Inverter inverter,
        bool applyAspectRatio, ValueType valueType, float? customSensitivityK = null) =>
        BuildKey((float)baseValue, isLandscape, ignoreMultiWindows, calcType,
            qualifier, inverter, applyAspectRatio, valueType, customSensitivityK);

    internal static bool HasCustomSensitivityKey(long key) =>
        ((key >>> 8) & 0xFFFFL) != 0xFFFFL;

    internal static bool ShouldBypassCache(long key)
    {
        var ct = (int)((key >> 27) & 0xFL);
        var hasAr = (key >> 63) != 0L;
        if (HasCustomSensitivityKey(key)) return false;

        // The imw flag routes through the full guard (multi-window suppression),
        // so the cheap multiply bypass must not apply to those keys.
        if ((key & 1L) != 0L) return false;

        var alwaysBypass = ct is CtPercent or CtScaled or CtDensity or CtDiagonal or CtInterpolated or CtPerimeter;
        var conditionalBypass = ct is CtPower or CtLogarithmic;
        if (!alwaysBypass && !conditionalBypass) return false;

        var q = (int)((key >> 6) & 0x3L);
        var inv = (int)((key >> 2) & 0xFL);
        var isDefaultSwPath = q == (int)DpQualifier.SmallWidth && inv == (int)Inverter.Default;
        if (alwaysBypass) return isDefaultSwPath;
        return isDefaultSwPath && !hasAr;
    }
}
