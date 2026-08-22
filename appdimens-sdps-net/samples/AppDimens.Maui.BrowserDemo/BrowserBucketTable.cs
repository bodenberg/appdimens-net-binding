using System.Net.Http.Json;
using System.Text.Json;
using AppDimens.Maui.Core;

namespace AppDimens.Maui.BrowserDemo;

/// <summary>
/// Lazy HTTP loader for the generated Android-parity bucket tables (layout v2):
/// buckets.json (sizes) + Dimens.Base.xaml + on-demand Dimens.{N}.xaml.
/// Mirrors <see cref="ResourceBucketManager"/> selection semantics.
/// </summary>
public sealed class BrowserBucketTable(HttpClient http)
{
    private int[] _sizes = [];
    private Dictionary<string, double>? _base;
    private readonly Dictionary<int, Dictionary<string, double>> _buckets = new();
    public bool Loaded { get; private set; }

    public async Task LoadAsync()
    {
        var json = await http.GetFromJsonAsync<JsonElement>("Generated/buckets.json");
        _sizes = json.GetProperty("sizes").EnumerateArray().Select(e => e.GetInt32()).ToArray();
        _base = ParseXaml(await http.GetStringAsync("Generated/Dimens.Base.xaml"));
        Loaded = true;
    }

    public static int SelectBucket(double metricDp, int[] sortedBuckets)
    {
        const int designBase = 300;
        var metric = (int)Math.Floor(metricDp);
        var selected = 0;
        foreach (var b in sortedBuckets)
        {
            if (b <= metric) selected = b; else break;
        }
        return selected == 0 ? designBase : selected;
    }

    /// <summary>Pre-fetches the bucket files covering the given axis metrics.</summary>
    public async Task PrefetchAsync(params double[] metricsDp)
    {
        if (!Loaded) return;
        foreach (var m in metricsDp)
        {
            var bucket = SelectBucket(m, _sizes);
            if (!_buckets.ContainsKey(bucket))
                _buckets[bucket] = ParseXaml(
                    await http.GetStringAsync($"Generated/Dimens.{bucket}.xaml"));
        }
    }

    /// <summary>Synchronous lookup against already-fetched tables.</summary>
    public bool TryGetCached(double metricDp, string key, out double value)
    {
        value = 0;
        if (!Loaded) return false;
        var bucket = SelectBucket(metricDp, _sizes);
        if (_buckets.TryGetValue(bucket, out var dict) && dict.TryGetValue(key, out value))
            return true;
        return _base!.TryGetValue(key, out value!);
    }

    internal static Dictionary<string, double> ParseXaml(string xaml)
    {
        var result = new Dictionary<string, double>();
        foreach (var line in xaml.Split('\n'))
        {
            var t = line.Trim();
            if (!t.StartsWith("<x:Double", StringComparison.Ordinal)) continue;
            var k0 = t.IndexOf("x:Key=\"", StringComparison.Ordinal);
            if (k0 < 0) continue;
            var k1 = t.IndexOf('"', k0 + 7);
            var vOpen = t.IndexOf('>', k1);
            if (vOpen < 0) continue;
            var vEnd = t.IndexOf('<', vOpen + 1);
            if (vEnd <= vOpen + 1) continue;
            var key = t.Substring(k0 + 7, k1 - k0 - 7);
            if (double.TryParse(t.Substring(vOpen + 1, vEnd - vOpen - 1),
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var v))
                result[key] = v;
        }
        return result;
    }
}
