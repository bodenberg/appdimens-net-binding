// AppDimens — Custom scaling strategies (portable engine). See CustomModel.cs.
#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace AppDimens.Net.Custom
{
    /// <summary>
    /// Everything a strategy needs for one window: the screen-only <see cref="Factor"/> plus the
    /// value-dependent parts. Computed once per snapshot and memoized on the strategy.
    /// </summary>
    public sealed class ResolvedScaling
    {
        internal ResolvedScaling(ScreenSnapshot screen, ScalingConfig config, float factor, IReadOnlyList<string> appliedRules,
            float axisDp, float referenceDp, float curveFactor, float aspectRatioMultiplier)
        {
            Screen = screen; Config = config; Factor = factor; AppliedRules = appliedRules;
            AxisDp = axisDp; ReferenceDp = referenceDp; CurveFactor = curveFactor; AspectRatioMultiplier = aspectRatioMultiplier;
        }

        public ScreenSnapshot Screen { get; }
        /// <summary>Effective config after the matching rules.</summary>
        public ScalingConfig Config { get; }
        public float Factor { get; }
        /// <summary>Names of the rules that matched, in order (unnamed rules as <c>#index</c>).</summary>
        public IReadOnlyList<string> AppliedRules { get; }
        public float AxisDp { get; }
        public float ReferenceDp { get; }
        public float CurveFactor { get; }
        public float AspectRatioMultiplier { get; }

        /// <summary>Scales <paramref name="baseValue"/> (dp) → dp.</summary>
        public float Scale(float baseValue)
        {
            var output = baseValue * Factor;
            var min = Config.MinValue;
            var max = Config.MaxValue;
            if ((min.HasValue || max.HasValue) && output != 0f)
            {
                // Bounds apply to the magnitude so negative offsets mirror positive ones; 0 stays 0.
                var magnitude = Math.Abs(output);
                if (min.HasValue && magnitude < min.Value) magnitude = min.Value;
                if (max.HasValue && magnitude > max.Value) magnitude = max.Value;
                output = output < 0f ? -magnitude : magnitude;
            }
            if (Config.Formula != null) output = Config.Formula.Evaluate(this, baseValue, output);
            else if (Config.Transform != null) output = Config.Transform(baseValue, output, Screen);
            return output;
        }

        /// <summary>
        /// Text size in <b>sp units</b> for toolkits that apply the system font scale themselves,
        /// corrected so the effective scale follows <see cref="ScalingConfig.FontScale"/>.
        /// </summary>
        public float ScaleSpUnits(float baseValue)
        {
            var system = Screen.FontScale;
            var correction = system > 0f ? Config.FontScale.Effective(system) / system : 1f;
            return Scale(baseValue) * correction;
        }

        /// <summary>Text size in dp after the font-scale policy (final size).</summary>
        public float ScaleSpDp(float baseValue) => Scale(baseValue) * Config.FontScale.Effective(Screen.FontScale);

        /// <summary>Human-readable breakdown, for logs and debug overlays.</summary>
        public string Explain(string strategyId)
        {
            var sb = new StringBuilder();
            sb.Append('[').Append(strategyId).Append("] ");
            sb.Append(Screen.Device.JsonName()).Append(' ');
            sb.Append(Config.Axis.JsonName()).Append('=').Append(Fmt(AxisDp)).Append("dp");
            sb.Append(" / ref ").Append(Fmt(ReferenceDp)).Append(" = ").Append(Fmt(AxisDp / ReferenceDp));
            sb.Append(" -> ").Append(Config.Curve).Append(" = ").Append(Fmt(CurveFactor));
            if (Config.Multiplier != 1f) sb.Append(" x ").Append(Fmt(Config.Multiplier));
            if (Config.AspectRatio) sb.Append(" x AR ").Append(Fmt(AspectRatioMultiplier));
            if (Config.MinFactor.HasValue || Config.MaxFactor.HasValue)
                sb.Append(" clamp[").Append(Config.MinFactor.HasValue ? Fmt(Config.MinFactor.Value) : "-").Append(", ")
                  .Append(Config.MaxFactor.HasValue ? Fmt(Config.MaxFactor.Value) : "-").Append(']');
            sb.Append(" => factor ").Append(Fmt(Factor));
            if (AppliedRules.Count > 0) sb.Append(" rules=[").Append(string.Join(", ", AppliedRules)).Append(']');
            return sb.ToString();
        }

        private static string Fmt(float v) => v.ToString("0.####", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// A named, user-defined scaling strategy. Create with <see cref="Define"/>, <see cref="Extend"/>,
    /// <see cref="ScalingStrategies.LoadJson"/> or a preset in <see cref="CustomPresets"/>.
    /// Immutable and thread-safe; the factor for a window is computed once and memoized, so resolving
    /// a value is one multiply (plus optional value clamps).
    /// </summary>
    public sealed class ScalingStrategy
    {
        private sealed class Memo
        {
            public Memo(object token, ResolvedScaling resolved) { Token = token; Resolved = resolved; }
            public readonly object Token;
            public readonly ResolvedScaling Resolved;
        }

        private volatile Memo? _memo;

        public ScalingStrategy(string id, ScalingConfig config)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("strategy id must not be blank");
            Id = id;
            Config = (config ?? throw new ArgumentNullException(nameof(config))).Validate();
            CacheId = ScalingStrategies.NextCacheId();
            IsValueDependent = Config.IsValueDependent;
        }

        public string Id { get; }
        public ScalingConfig Config { get; }

        /// <summary>
        /// Process-unique id packed into cache keys (16 bits); <c>-1</c> when the registry ran out of
        /// ids — such a strategy still works but never writes the shared cache.
        /// </summary>
        public int CacheId { get; }

        /// <summary>True when results depend on the value, not only on the screen.</summary>
        public bool IsValueDependent { get; }

        /// <summary>Creates <b>and registers</b> a strategy with the builder.</summary>
        /// <example><code>
        /// var brand = ScalingStrategy.Define("brand", s => {
        ///     s.SetReference(360, 640);
        ///     s.Curve = ScalingCurve.Power(0.8f);
        ///     s.Clamp(0.85f, 1.6f);
        ///     s.On(new[] { DeviceClass.Tablet }, r => r.Curve = ScalingCurve.Logarithmic(0.4f));
        /// });
        /// </code></example>
        public static ScalingStrategy Define(string id, Action<ScalingStrategyBuilder> configure, bool register = true)
        {
            var builder = new ScalingStrategyBuilder();
            configure(builder);
            var s = new ScalingStrategy(id, builder.Build());
            return register ? ScalingStrategies.Register(s) : s;
        }

        /// <summary>Resolves (memoized) for <paramref name="screen"/>.</summary>
        public ResolvedScaling Resolve(ScreenSnapshot screen) => Resolve(screen, () => screen);

        /// <summary>
        /// Resolves for a platform snapshot <paramref name="token"/> (e.g. <c>DimenMetrics</c>), building
        /// the <see cref="ScreenSnapshot"/> only on a memo miss. The token must have value equality.
        /// </summary>
        public ResolvedScaling Resolve(object token, Func<ScreenSnapshot> snapshot)
        {
            var m = _memo;
            if (m != null && (ReferenceEquals(m.Token, token) || m.Token.Equals(token))) return m.Resolved;
            var resolved = ScalingEngine.Resolve(Config, snapshot());
            _memo = new Memo(token, resolved);
            return resolved;
        }

        public float Factor(ScreenSnapshot screen) => Resolve(screen).Factor;
        public float Scale(float baseValue, ScreenSnapshot screen) => Resolve(screen).Scale(baseValue);
        public string Explain(ScreenSnapshot screen) => Resolve(screen).Explain(Id);

        /// <summary>New strategy that starts from this one; rules of both are kept (parent first).</summary>
        public ScalingStrategy Extend(string id, Action<ScalingStrategyBuilder> configure, bool register = true)
        {
            var builder = new ScalingStrategyBuilder(Config);
            configure(builder);
            var s = new ScalingStrategy(id, builder.Build());
            return register ? ScalingStrategies.Register(s) : s;
        }

        /// <summary>Same strategy on another axis (e.g. heights). Id: <c>"&lt;id&gt;@&lt;axis&gt;"</c>.</summary>
        public ScalingStrategy WithAxis(ScalingAxis axis)
        {
            if (axis == Config.Axis) return this;
            var derivedId = Id + "@" + axis.JsonName();
            return ScalingStrategies.Derived(derivedId, () => new ScalingStrategy(derivedId, Config.With(axis: axis)));
        }

        /// <summary>JSON for this strategy (throws for lambda curves/transforms).</summary>
        public string ToJson() => ScalingStrategyJson.Encode(this);

        public override string ToString() => $"ScalingStrategy({Id})";
    }

    /// <summary>Pure math shared by every platform binding.</summary>
    internal static class ScalingEngine
    {
        /// <summary>Default AR sensitivity — same constant as the built-in strategies (0.08 / 30).</summary>
        public const float SensitivityDefault = 0.08f / 30f;
        public const float ReferenceAspectRatio = 1.78f;

        public static ResolvedScaling Resolve(ScalingConfig baseConfig, ScreenSnapshot screen)
        {
            var cfg = baseConfig;
            var applied = new List<string>(0);
            for (var i = 0; i < baseConfig.Rules.Count; i++)
            {
                var rule = baseConfig.Rules[i];
                if (!rule.Condition.Matches(screen)) continue;
                cfg = cfg.Apply(rule.Patch);
                applied.Add(rule.Name ?? "#" + i.ToString(CultureInfo.InvariantCulture));
            }
            var axisDp = screen.Read(cfg.Axis);
            var refDp = cfg.Reference.Read(cfg.Axis);
            if (cfg.IgnoreMultiWindow && screen.IsMultiWindow)
                return new ResolvedScaling(screen, cfg, 1f, applied, axisDp, refDp, 1f, 1f);
            var ratio = refDp > 0f ? axisDp / refDp : 1f;
            var curveFactor = Curve(cfg.Curve, ratio, axisDp, refDp, cfg.Reference, screen);
            var factor = curveFactor * cfg.Multiplier;
            var arMul = 1f;
            if (cfg.AspectRatio)
            {
                var k = cfg.Sensitivity ?? SensitivityDefault;
                arMul = 1f + k * (float)Math.Log(screen.AspectRatio / ReferenceAspectRatio);
                factor *= arMul;
            }
            if (cfg.MinFactor.HasValue && factor < cfg.MinFactor.Value) factor = cfg.MinFactor.Value;
            if (cfg.MaxFactor.HasValue && factor > cfg.MaxFactor.Value) factor = cfg.MaxFactor.Value;
            if (!float.IsFinite(factor)) throw new InvalidOperationException($"custom strategy produced a non-finite factor ({factor})");
            return new ResolvedScaling(screen, cfg, factor, applied, axisDp, refDp, curveFactor, arMul);
        }

        public static float Curve(ScalingCurve curve, float ratio, float axisDp, float refDp, ReferenceScreen reference, ScreenSnapshot screen)
        {
            switch (curve)
            {
                case PowerCurve p: return ratio > 0f ? (float)Math.Pow(ratio, p.Exponent) : 0f;
                case LogarithmicCurve l: return ratio > 0f ? 1f + l.K * (float)Math.Log(ratio) : 1f;
                case InterpolatedCurve t: return 1f + (ratio - 1f) * t.T;
                case ConstantCurve c: return c.Factor;
                case BreakpointsCurve b: return Interpolate(b.Points, axisDp);
                case CustomCurve f: return f.Function(ratio, screen);
                case ExpressionCurve x:
                    return (float)x.Body.Evaluate(ScalingExpression.Variables(screen, ratio, axisDp, refDp, reference));
                default: return ratio; // Linear
            }
        }

        private static float Interpolate(IReadOnlyList<Breakpoint> points, float x)
        {
            var first = points[0];
            if (x <= first.At) return first.Factor;
            for (var i = 1; i < points.Count; i++)
            {
                var b = points[i];
                if (x <= b.At)
                {
                    var a = points[i - 1];
                    var span = b.At - a.At;
                    return span <= 0f ? b.Factor : a.Factor + (b.Factor - a.Factor) * ((x - a.At) / span);
                }
            }
            return points[points.Count - 1].Factor;
        }
    }

    /// <summary>Process-wide registry of strategies by id, plus the default used by <c>16.Cdp()</c>.</summary>
    public static class ScalingStrategies
    {
        /// <summary>Largest id that fits the 16-bit custom cache key field.</summary>
        public const int MaxCacheId = 0xFFFF;

        private static readonly ConcurrentDictionary<string, ScalingStrategy> ById = new ConcurrentDictionary<string, ScalingStrategy>();
        private static readonly ConcurrentDictionary<string, ScalingStrategy> DerivedCache = new ConcurrentDictionary<string, ScalingStrategy>();
        private static int _cacheIds = -1;
        private static volatile ScalingStrategy? _default;

        internal static int NextCacheId()
        {
            var next = Interlocked.Increment(ref _cacheIds);
            return next >= 0 && next <= MaxCacheId ? next : -1;
        }

        /// <summary>
        /// Strategy used when none is passed and none is provided by scope.
        /// Defaults to <see cref="BuiltInStrategies.Scaled"/> (same numbers as <c>Sdp</c>).
        /// </summary>
        public static ScalingStrategy Default
        {
            get => _default ?? BuiltInStrategies.Scaled;
            set => _default = value;
        }

        /// <summary>Registers (or replaces) <paramref name="strategy"/> under its id.</summary>
        public static ScalingStrategy Register(ScalingStrategy strategy)
        {
            ById[strategy.Id] = strategy;
            return strategy;
        }

        /// <summary>Strategy for <paramref name="id"/> (registered, built-in or preset), or <c>null</c>.</summary>
        public static ScalingStrategy? Get(string id) =>
            ById.TryGetValue(id, out var s) ? s : BuiltInStrategies.ByName(id) ?? CustomPresets.ByName(id);

        /// <summary>Like <see cref="Get"/> but throws a descriptive error.</summary>
        public static ScalingStrategy Require(string id) =>
            Get(id) ?? throw new KeyNotFoundException(
                $"No scaling strategy '{id}'. Registered: [{string.Join(", ", ById.Keys.OrderBy(k => k, StringComparer.Ordinal))}]");

        /// <summary>Ids registered by the app (built-ins/presets excluded).</summary>
        public static IReadOnlyCollection<string> Ids => ById.Keys.ToArray();

        public static void Unregister(string id) => ById.TryRemove(id, out _);

        /// <summary>
        /// Parses a strategies document (see <c>custom-strategy.schema.json</c>), registers every
        /// strategy and applies its <c>default</c>. Returns the parsed strategies by id.
        /// </summary>
        public static IReadOnlyDictionary<string, ScalingStrategy> LoadJson(string json)
        {
            var doc = ScalingStrategyJson.Decode(json);
            foreach (var s in doc.Strategies.Values) Register(s);
            if (doc.DefaultId != null)
                Default = doc.Strategies.TryGetValue(doc.DefaultId, out var d) ? d : Require(doc.DefaultId);
            return doc.Strategies;
        }

        /// <summary>Encodes the registered strategies (plus the default id) as a document.</summary>
        public static string ExportJson() =>
            ScalingStrategyJson.EncodeDocument(ById.Values.OrderBy(s => s.Id, StringComparer.Ordinal).ToArray(), _default?.Id);

        internal static ScalingStrategy Derived(string id, Func<ScalingStrategy> create) =>
            DerivedCache.GetOrAdd(id, _ => create());

        /// <summary>Test helper: clears registrations and the default.</summary>
        public static void ResetForTesting()
        {
            ById.Clear();
            DerivedCache.Clear();
            _default = null;
        }
    }

    /// <summary>
    /// The built-in strategies expressed as custom configs — starting points for
    /// <see cref="ScalingStrategy.Extend"/> and for JSON <c>"extends": "power"</c>. Their factors match
    /// the built-in modules (default smallest-width path, no aspect ratio).
    /// </summary>
    public static class BuiltInStrategies
    {
        /// <summary><c>sw / 300</c> — same as <c>Sdp</c>.</summary>
        public static readonly ScalingStrategy Scaled = new ScalingStrategy("scaled", ScalingConfig.Default);
        /// <summary><c>(sw / 300)^0.75</c> — same as the power module.</summary>
        public static readonly ScalingStrategy Power = new ScalingStrategy("power", new ScalingConfig(curve: ScalingCurve.Power(0.75f)));
        /// <summary><c>1 + 0.4·ln(sw / 300)</c> — same as the logarithmic module.</summary>
        public static readonly ScalingStrategy Logarithmic = new ScalingStrategy("logarithmic", new ScalingConfig(curve: ScalingCurve.Logarithmic(0.4f)));
        /// <summary><c>1 + (sw / 300 − 1)·0.5</c> — same as the interpolated module.</summary>
        public static readonly ScalingStrategy Interpolated = new ScalingStrategy("interpolated", new ScalingConfig(curve: ScalingCurve.Interpolated(0.5f)));
        /// <summary><c>√(w² + h²) / 611.63</c> — same as the diagonal module.</summary>
        public static readonly ScalingStrategy Diagonal = new ScalingStrategy("diagonal", new ScalingConfig(axis: ScalingAxis.Diagonal));
        /// <summary><c>(w + h) / 833</c> — same as the perimeter module.</summary>
        public static readonly ScalingStrategy Perimeter = new ScalingStrategy("perimeter", new ScalingConfig(axis: ScalingAxis.Perimeter));
        /// <summary>Plain dp (factor 1).</summary>
        public static readonly ScalingStrategy Fixed = new ScalingStrategy("fixed", new ScalingConfig(curve: ScalingCurve.Constant(1f)));

        public static IReadOnlyList<ScalingStrategy> All => new[] { Scaled, Power, Logarithmic, Interpolated, Diagonal, Perimeter, Fixed };

        public static ScalingStrategy? ByName(string name) =>
            All.FirstOrDefault(s => string.Equals(s.Id, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Ready-made strategies for common product needs. Use as-is, or <c>Extend</c> them.</summary>
    public static class CustomPresets
    {
        /// <summary>Linear on phones; damped (power 0.5) on tablets/desktop. Factor capped at 1.8.</summary>
        public static readonly ScalingStrategy TabletFriendly = ScalingStrategy.Define("tabletFriendly", s =>
        {
            s.Clamp(max: 1.8f);
            s.On(new[] { DeviceClass.Tablet, DeviceClass.Desktop }, r => r.Curve = ScalingCurve.Power(0.5f), "large");
        }, register: false);

        /// <summary>Designed on a compact phone: grows at half speed and stays within 0.9–1.4×.</summary>
        public static readonly ScalingStrategy CompactFirst = ScalingStrategy.Define("compactFirst", s =>
        {
            s.SetReference(360f, 640f);
            s.Curve = ScalingCurve.Interpolated(0.5f);
            s.Clamp(0.9f, 1.4f);
        }, register: false);

        /// <summary>10-foot UI: on TV, scale by window height against a 540 dp frame, 1×–2×.</summary>
        public static readonly ScalingStrategy TvSafe = ScalingStrategy.Define("tvSafe", s =>
            s.On(new[] { DeviceClass.Tv }, r =>
            {
                r.Axis = ScalingAxis.Height;
                r.SetReference(960f, 540f);
                r.Clamp(1f, 2f);
            }, "tv"), register: false);

        /// <summary>Wearables: scale against a 192 dp face and keep within ±15 %.</summary>
        public static readonly ScalingStrategy Wearable = ScalingStrategy.Define("wearable", s =>
            s.On(new[] { DeviceClass.Watch }, r =>
            {
                r.Axis = ScalingAxis.Min;
                r.SetReference(192f, 192f);
                r.Clamp(0.85f, 1.15f);
            }, "watch"), register: false);

        /// <summary>Game HUD: shorter side, sublinear growth, aspect-ratio aware, fixed-size text.</summary>
        public static readonly ScalingStrategy GameHud = ScalingStrategy.Define("gameHud", s =>
        {
            s.Axis = ScalingAxis.Min;
            s.Curve = ScalingCurve.Power(0.85f);
            s.EnableAspectRatio();
            s.Clamp(0.75f, 2.5f);
            s.IgnoreFontScale();
        }, register: false);

        /// <summary>Accessible text: follows the user font scale up to 1.3× and damps screen growth.</summary>
        public static readonly ScalingStrategy ReadableText = ScalingStrategy.Define("readableText", s =>
        {
            s.Curve = ScalingCurve.Logarithmic(0.4f);
            s.ClampFontScale(1.3f);
        }, register: false);

        public static IReadOnlyList<ScalingStrategy> All => new[] { TabletFriendly, CompactFirst, TvSafe, Wearable, GameHud, ReadableText };

        public static ScalingStrategy? ByName(string name) =>
            All.FirstOrDefault(s => string.Equals(s.Id, name, StringComparison.OrdinalIgnoreCase));
    }
}
