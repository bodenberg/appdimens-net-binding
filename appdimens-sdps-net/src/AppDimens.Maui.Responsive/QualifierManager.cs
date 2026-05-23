using System.Text.Json;
using System.Text.RegularExpressions;
using AppDimens.Maui.Core;
using static AppDimens.Maui.Core.AppDimensLogging;

namespace AppDimens.Maui.Responsive;

public static class QualifierManager
{
    public static int SelectBucket(double metricDp, IReadOnlyList<int> sortedBuckets)
    {
        if (sortedBuckets.Count == 0) return (int)AppDimensConstants.DesignBaseDp;

        var metric = (int)Math.Floor(metricDp);
        var selected = 0;
        foreach (var bucket in sortedBuckets)
        {
            if (bucket <= metric)
                selected = bucket;
            else
                break;
        }
        return selected == 0 ? (int)AppDimensConstants.DesignBaseDp : selected;
    }

    public static double GetMetricForQualifier(DpQualifier qualifier, ScreenMetricsSnapshot metrics) =>
        ScaleEngine.GetMetricDp(qualifier, metrics);
}

public sealed class BucketRegistry
{
    private static readonly Regex MergedBucketFile = new(@"^Dimens\.(\d+)$", RegexOptions.Compiled);

    public IReadOnlyList<int> Sizes { get; }
    public int LayoutVersion { get; }
    public Dictionary<string, double> BaseValues { get; } = new();
    public Dictionary<int, Dictionary<string, double>> BucketValues { get; } = new();

    /// <summary>Per-axis bucket tables used when <c>layoutVersion</c> is below 2.</summary>
    public Dictionary<string, Dictionary<string, double>> LegacyBaseValues { get; } = new();
    public Dictionary<string, Dictionary<int, Dictionary<string, double>>> LegacyBucketValues { get; } = new();

    public BucketRegistry(IReadOnlyList<int> sizes, int layoutVersion = 2)
    {
        Sizes = sizes;
        LayoutVersion = layoutVersion;
        if (layoutVersion < 2)
        {
            foreach (var axis in new[] { "sw", "w", "h" })
            {
                LegacyBaseValues[axis] = new Dictionary<string, double>();
                LegacyBucketValues[axis] = new Dictionary<int, Dictionary<string, double>>();
            }
        }
    }

    public static BucketRegistry LoadFromGenerated(string generatedDir)
    {
        var jsonPath = Path.Combine(generatedDir, "buckets.json");
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException("Run scripts/generate-dimens.py first.", jsonPath);

        var json = JsonDocument.Parse(File.ReadAllText(jsonPath));
        var root = json.RootElement;
        var layoutVersion = root.TryGetProperty("layoutVersion", out var lv) ? lv.GetInt32() : 1;
        var sizes = root.GetProperty("sizes").EnumerateArray()
            .Select(e => e.GetInt32()).ToList();

        var registry = new BucketRegistry(sizes, layoutVersion);

        if (layoutVersion >= 2)
            registry.LoadMergedLayout(generatedDir);
        else
            registry.LoadLegacyLayout(generatedDir);

        if (layoutVersion < 2 && registry.HasLegacyAxisFolders(generatedDir))
        {
            Warning("Loading legacy sw/w/h dimens layout; regenerate with generate-dimens.py for layout v2.");
            registry.LoadLegacyLayout(generatedDir);
        }

        if (registry.BucketValues.Count == 0 && File.Exists(Path.Combine(generatedDir, "Dimens.Base.xaml")))
            registry.LoadMergedLayout(generatedDir);

        if (registry.BucketValues.Count == 0 && registry.LegacyBucketValues.Count == 0)
            throw new InvalidOperationException(
                $"No dimens resources found under {generatedDir}. Run scripts/generate-dimens.py.");

        return registry;
    }

    private bool HasLegacyAxisFolders(string generatedDir) =>
        new[] { "sw", "w", "h" }.Any(a => Directory.Exists(Path.Combine(generatedDir, a)));

    private void LoadMergedLayout(string generatedDir)
    {
        BaseValues.Clear();
        BucketValues.Clear();

        var basePath = Path.Combine(generatedDir, "Dimens.Base.xaml");
        if (File.Exists(basePath))
            foreach (var kv in XamlDimenParser.Parse(basePath))
                BaseValues[kv.Key] = kv.Value;

        foreach (var file in Directory.GetFiles(generatedDir, "Dimens.*.xaml"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (name == "Dimens.Base") continue;
            var m = MergedBucketFile.Match(name);
            if (!m.Success || !int.TryParse(m.Groups[1].Value, out var bucket)) continue;
            BucketValues[bucket] = XamlDimenParser.Parse(file);
        }
    }

    private void LoadLegacyLayout(string generatedDir)
    {
        foreach (var axis in new[] { "sw", "w", "h" })
        {
            var basePath = Path.Combine(generatedDir, $"Dimens.Base.{axis}.xaml");
            if (File.Exists(basePath))
                LegacyBaseValues[axis] = XamlDimenParser.Parse(basePath);

            var axisDir = Path.Combine(generatedDir, axis);
            if (!Directory.Exists(axisDir)) continue;

            foreach (var file in Directory.GetFiles(axisDir, "*.xaml"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var bucketStr = name.Replace($"Dimens.{axis}", "");
                if (int.TryParse(bucketStr, out var bucket))
                    LegacyBucketValues[axis][bucket] = XamlDimenParser.Parse(file);
            }
        }
    }

    public bool TryGetValue(DpQualifier qualifier, int bucket, string key, out double value)
    {
        if (BucketValues.Count > 0)
            return TryGetMerged(bucket, key, out value);

        var axis = qualifier switch
        {
            DpQualifier.Height => "h",
            DpQualifier.Width => "w",
            _ => "sw",
        };

        if (bucket <= 0 || !LegacyBucketValues[axis].TryGetValue(bucket, out var dict))
            return TryDictWithLegacySuffix(LegacyBaseValues[axis], key, qualifier, out value);

        if (TryDictWithLegacySuffix(dict, key, qualifier, out value))
            return true;

        return TryDictWithLegacySuffix(LegacyBaseValues[axis], key, qualifier, out value);
    }

    private bool TryGetMerged(int bucket, string key, out double value)
    {
        if (bucket <= 0 || !BucketValues.TryGetValue(bucket, out var dict))
            return TryDict(BaseValues, key, out value);

        if (TryDict(dict, key, out value))
            return true;

        return TryDict(BaseValues, key, out value);
    }

    private static bool TryDict(Dictionary<string, double> dict, string key, out double value)
    {
        if (dict.TryGetValue(key, out value))
            return true;
        value = default;
        return false;
    }

    private static bool TryDictWithLegacySuffix(
        Dictionary<string, double> dict, string key, DpQualifier qualifier, out double value)
    {
        if (TryDict(dict, key, out value))
            return true;
        return TryDict(dict, ScaleEngine.BuildLegacySuffixedKey(key, qualifier), out value);
    }

    public double GetOneUnit(DpQualifier qualifier, int bucket) =>
        TryGetValue(qualifier, bucket, "_1", out var v) ? v : 1.0;
}

public static class XamlDimenParser
{
    public static Dictionary<string, double> Parse(string path)
    {
        var result = new Dictionary<string, double>();
        foreach (var line in File.ReadLines(path))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("<x:Double", StringComparison.Ordinal)) continue;
            var keyStart = trimmed.IndexOf("x:Key=\"", StringComparison.Ordinal);
            if (keyStart < 0) continue;
            var keyEnd = trimmed.IndexOf('"', keyStart + 7);
            var valueOpen = trimmed.IndexOf('>', keyEnd);
            if (valueOpen < 0) continue;
            var valStart = valueOpen + 1;
            var valEnd = trimmed.IndexOf('<', valStart);
            if (valEnd <= valStart) continue;
            var key = trimmed.Substring(keyStart + 7, keyEnd - keyStart - 7);
            var valStr = trimmed.Substring(valStart, valEnd - valStart);
            if (double.TryParse(valStr, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var val))
                result[key] = val;
        }
        return result;
    }
}

public sealed class ResourceBucketManager
{
    private readonly BucketRegistry _registry;
    private readonly object _lock = new();
    private int _signature = int.MinValue;
    private Dictionary<DpQualifier, int> _activeBuckets = new();

    public ResourceBucketManager(BucketRegistry registry) => _registry = registry;

    public void EnsureUpToDate(ScreenMetricsSnapshot metrics)
    {
        var sig = HashCode.Combine(
            (int)metrics.SmallestDp, (int)metrics.WidthDp, (int)metrics.HeightDp);
        if (sig == _signature) return;

        lock (_lock)
        {
            if (sig == _signature) return;
            _activeBuckets = new Dictionary<DpQualifier, int>
            {
                [DpQualifier.SmallWidth] = QualifierManager.SelectBucket(metrics.SmallestDp, _registry.Sizes),
                [DpQualifier.Width] = QualifierManager.SelectBucket(metrics.WidthDp, _registry.Sizes),
                [DpQualifier.Height] = QualifierManager.SelectBucket(metrics.HeightDp, _registry.Sizes),
            };
            _signature = sig;
        }
    }

    public bool TryGetDimen(DpQualifier qualifier, string key, out double value)
    {
        lock (_lock)
        {
            var bucket = _activeBuckets.GetValueOrDefault(qualifier, (int)AppDimensConstants.DesignBaseDp);
            return _registry.TryGetValue(qualifier, bucket, key, out value);
        }
    }

    public double GetOneUnit(DpQualifier qualifier)
    {
        lock (_lock)
        {
            var bucket = _activeBuckets.GetValueOrDefault(qualifier, (int)AppDimensConstants.DesignBaseDp);
            return _registry.GetOneUnit(qualifier, bucket);
        }
    }

    public int GetActiveBucket(DpQualifier qualifier)
    {
        lock (_lock)
            return _activeBuckets.GetValueOrDefault(qualifier, (int)AppDimensConstants.DesignBaseDp);
    }
}

public sealed class ResponsiveManager
{
    private readonly DimensionCache _cache;
    private readonly AspectRatioFactors _aspectRatio;
    private readonly ResourceBucketManager _buckets;

    public ResponsiveManager(DimensionCache cache, AspectRatioFactors aspectRatio, ResourceBucketManager buckets)
    {
        _cache = cache;
        _aspectRatio = aspectRatio;
        _buckets = buckets;
    }

    public void InvalidateAll()
    {
        _cache.Invalidate();
        _aspectRatio.ResetForTests();
    }

    public void OnMetricsChanged(ScreenMetricsSnapshot metrics)
    {
        _cache.Invalidate();
        _buckets.EnsureUpToDate(metrics);
        _aspectRatio.EnsureUpToDate(metrics, q => _buckets.GetOneUnit(q));
    }
}
