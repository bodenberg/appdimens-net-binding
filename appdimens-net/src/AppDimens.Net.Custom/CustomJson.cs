// AppDimens — Custom scaling strategies (portable engine). See CustomModel.cs.
#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AppDimens.Net.Custom
{
    /// <summary>Parsed strategies document.</summary>
    public sealed class ScalingStrategyDocument
    {
        internal ScalingStrategyDocument(IReadOnlyDictionary<string, ScalingStrategy> strategies, string? defaultId)
        {
            Strategies = strategies;
            DefaultId = defaultId;
        }

        public IReadOnlyDictionary<string, ScalingStrategy> Strategies { get; }
        public string? DefaultId { get; }
    }

    /// <summary>
    /// JSON codec for the cross-platform strategy format
    /// (<c>appdimens-skills/schemas/custom-strategy.schema.json</c>). Self-contained (no
    /// System.Text.Json) so the same code runs in .NET, Unity and Godot.
    /// <code>
    /// { "version": 1, "default": "brand",
    ///   "strategies": [ { "id": "brand", "extends": "power",
    ///                     "reference": { "width": 360, "height": 640 },
    ///                     "curve": { "type": "power", "exponent": 0.8 },
    ///                     "clamp": { "min": 0.85, "max": 1.6 },
    ///                     "rules": [ { "when": { "device": ["tablet"] },
    ///                                  "set": { "curve": { "type": "logarithmic", "k": 0.4 } } } ] } ] }
    /// </code>
    /// </summary>
    public static class ScalingStrategyJson
    {
        public const int FormatVersion = 1;

        /// <summary>
        /// Decodes a document (<c>{"strategies": [...]}</c>), a single strategy object, or an array.
        /// <c>extends</c> may name another strategy of the same document, a registered strategy,
        /// a built-in (<c>scaled</c>, <c>power</c>, …) or a preset (<c>tabletFriendly</c>, …).
        /// </summary>
        public static ScalingStrategyDocument Decode(string json)
        {
            var root = new MiniJson(json).Parse();
            List<object?> items;
            string? defaultId = null;
            if (root is Dictionary<string, object?> map)
            {
                if (map.ContainsKey("strategies"))
                {
                    var version = map.TryGetValue("version", out var v) && v is double d ? (int)d : FormatVersion;
                    if (version > FormatVersion) throw new ArgumentException($"Unsupported strategies format version {version}");
                    items = List(map["strategies"], "strategies");
                    defaultId = map.TryGetValue("default", out var def) ? def as string : null;
                }
                else items = new List<object?> { map };
            }
            else if (root is List<object?> list) items = list;
            else throw new ArgumentException("Expected a JSON object or array");

            var raw = new Dictionary<string, Dictionary<string, object?>>();
            var order = new List<string>();
            for (var i = 0; i < items.Count; i++)
            {
                var obj = items[i] as Dictionary<string, object?> ?? throw new ArgumentException($"strategies[{i}] must be an object");
                var id = Get(obj, "id") as string ?? throw new ArgumentException($"strategies[{i}].id is required");
                if (raw.ContainsKey(id)) throw new ArgumentException($"Duplicate strategy id '{id}'");
                raw[id] = obj;
                order.Add(id);
            }

            var built = new Dictionary<string, ScalingStrategy>();
            ScalingStrategy Build(string id, List<string> stack)
            {
                if (built.TryGetValue(id, out var done)) return done;
                if (stack.Contains(id)) throw new ArgumentException($"Cyclic 'extends': {string.Join(" -> ", stack.Append(id))}");
                var obj = raw[id];
                var parentId = Get(obj, "extends") as string;
                ScalingConfig parent;
                if (parentId == null) parent = ScalingConfig.Default;
                else if (raw.ContainsKey(parentId)) parent = Build(parentId, stack.Append(id).ToList()).Config;
                else parent = (ScalingStrategies.Get(parentId)
                    ?? throw new ArgumentException($"'{id}' extends unknown strategy '{parentId}'")).Config;
                var strategy = new ScalingStrategy(id, ReadConfig(obj, parent, id));
                built[id] = strategy;
                return strategy;
            }

            var ordered = new Dictionary<string, ScalingStrategy>();
            foreach (var id in order) ordered[id] = Build(id, new List<string>());
            return new ScalingStrategyDocument(ordered, defaultId);
        }

        /// <summary>Decodes and returns the single (or first) strategy.</summary>
        public static ScalingStrategy DecodeStrategy(string json) =>
            Decode(json).Strategies.Values.FirstOrDefault() ?? throw new ArgumentException("No strategy in JSON");

        /// <summary>Encodes one strategy. Lambda curves/transforms cannot be encoded.</summary>
        public static string Encode(ScalingStrategy strategy)
        {
            var sb = new StringBuilder();
            WriteStrategy(sb, strategy.Id, strategy.Config);
            return sb.ToString();
        }

        /// <summary>Encodes a full document.</summary>
        public static string EncodeDocument(IEnumerable<ScalingStrategy> strategies, string? defaultId = null)
        {
            var sb = new StringBuilder();
            sb.Append("{\"version\":").Append(FormatVersion);
            if (defaultId != null) sb.Append(",\"default\":").Append(Quote(defaultId));
            sb.Append(",\"strategies\":[");
            var first = true;
            foreach (var s in strategies)
            {
                if (!first) sb.Append(',');
                first = false;
                WriteStrategy(sb, s.Id, s.Config);
            }
            return sb.Append("]}").ToString();
        }

        // ── decoding ────────────────────────────────────────────────────────

        private static object? Get(Dictionary<string, object?> obj, string key) => obj.TryGetValue(key, out var v) ? v : null;

        private static ScalingConfig ReadConfig(Dictionary<string, object?> obj, ScalingConfig parent, string path)
        {
            var cfg = parent.Apply(ReadPatch(obj, path));
            // Explicit nulls at the root reset inherited values.
            if (obj.TryGetValue("clamp", out var c) && c == null) cfg = cfg.WithFactorBounds(null, null);
            if (obj.TryGetValue("valueClamp", out var vc) && vc == null) cfg = cfg.WithValueBounds(null, null);
            if (obj.TryGetValue("formula", out var fm) && fm == null) cfg = cfg.WithFormula(null);
            var rules = new List<ScalingRule>(parent.Rules);
            var rawRules = Get(obj, "rules");
            if (rawRules != null)
            {
                var list = List(rawRules, path + ".rules");
                for (var i = 0; i < list.Count; i++)
                {
                    var rp = $"{path}.rules[{i}]";
                    var rule = list[i] as Dictionary<string, object?> ?? throw new ArgumentException($"{rp} must be an object");
                    var when = Get(rule, "when") as Dictionary<string, object?> ?? new Dictionary<string, object?>();
                    var set = Get(rule, "set") as Dictionary<string, object?> ?? throw new ArgumentException($"{rp}.set is required");
                    rules.Add(new ScalingRule(ReadCondition(when, rp + ".when"), ReadPatch(set, rp + ".set"), Get(rule, "name") as string));
                }
            }
            return cfg.WithRules(rules);
        }

        private static ScalingPatch ReadPatch(Dictionary<string, object?> obj, string path)
        {
            var clamp = Get(obj, "clamp") as Dictionary<string, object?>;
            var valueClamp = Get(obj, "valueClamp") as Dictionary<string, object?>;
            var ar = Get(obj, "aspectRatio");
            ReferenceScreen? reference = null;
            if (Get(obj, "reference") is Dictionary<string, object?> r)
                reference = new ReferenceScreen(Num(Get(r, "width"), path + ".reference.width") ?? 300f,
                    Num(Get(r, "height"), path + ".reference.height") ?? 533f);
            bool? aspect;
            switch (ar)
            {
                case null: aspect = null; break;
                case bool b: aspect = b; break;
                case Dictionary<string, object?> m: aspect = Get(m, "enabled") as bool? ?? true; break;
                default: throw new ArgumentException($"{path}.aspectRatio must be a boolean or object");
            }
            var sensitivity = ar is Dictionary<string, object?> arMap
                ? Num(Get(arMap, "sensitivity"), path + ".aspectRatio.sensitivity") ?? Num(Get(obj, "sensitivity"), path + ".sensitivity")
                : Num(Get(obj, "sensitivity"), path + ".sensitivity");
            var axis = Get(obj, "axis") as string;
            var curve = Get(obj, "curve");
            var fontScale = Get(obj, "fontScale");
            Formula? formula = null;
            switch (Get(obj, "formula"))
            {
                case null: break;
                case string src: formula = Wrap(path + ".formula", () => new Formula(src)); break;
                default: throw new ArgumentException($"{path}.formula must be a string");
            }
            return new ScalingPatch(
                reference: reference,
                axis: axis == null ? (ScalingAxis?)null : CustomNames.AxisFromJson(axis),
                curve: curve == null ? null : ReadCurve(curve, path + ".curve"),
                multiplier: Num(Get(obj, "multiplier"), path + ".multiplier"),
                minFactor: clamp == null ? null : Num(Get(clamp, "min"), path + ".clamp.min"),
                maxFactor: clamp == null ? null : Num(Get(clamp, "max"), path + ".clamp.max"),
                minValue: valueClamp == null ? null : Num(Get(valueClamp, "min"), path + ".valueClamp.min"),
                maxValue: valueClamp == null ? null : Num(Get(valueClamp, "max"), path + ".valueClamp.max"),
                aspectRatio: aspect,
                sensitivity: sensitivity,
                fontScale: fontScale == null ? null : ReadFontScale(fontScale, path + ".fontScale"),
                ignoreMultiWindow: Get(obj, "ignoreMultiWindow") as bool?,
                formula: formula);
        }

        private static T Wrap<T>(string path, Func<T> create)
        {
            try { return create(); }
            catch (ArgumentException e) { throw new ArgumentException($"{path}: {e.Message}", e); }
        }

        private static ScalingCurve ReadCurve(object value, string path)
        {
            if (value is string s) return ReadCurve(new Dictionary<string, object?> { ["type"] = s }, path);
            var obj = value as Dictionary<string, object?> ?? throw new ArgumentException($"{path} must be an object or string");
            var type = Get(obj, "type") as string ?? throw new ArgumentException($"{path}.type is required");
            switch (type)
            {
                case "linear": return ScalingCurve.Linear;
                case "power": return ScalingCurve.Power(Num(Get(obj, "exponent"), path + ".exponent") ?? 0.75f);
                case "logarithmic": return ScalingCurve.Logarithmic(Num(Get(obj, "k"), path + ".k") ?? 0.4f);
                case "interpolated": return ScalingCurve.Interpolated(Num(Get(obj, "t"), path + ".t") ?? 0.5f);
                case "constant": return ScalingCurve.Constant(Num(Get(obj, "factor"), path + ".factor") ?? 1f);
                case "expression":
                    var src = Get(obj, "expression") as string ?? throw new ArgumentException($"{path}.expression is required");
                    return Wrap(path + ".expression", () => ScalingCurve.Expression(src));
                case "breakpoints":
                    var points = List(Get(obj, "points"), path + ".points");
                    return ScalingCurve.Breakpoints(points.Select((p, i) =>
                    {
                        var pp = $"{path}.points[{i}]";
                        var point = p as Dictionary<string, object?> ?? throw new ArgumentException($"{pp} must be an object");
                        return new Breakpoint(
                            Num(Get(point, "at"), pp + ".at") ?? throw new ArgumentException($"{pp}.at is required"),
                            Num(Get(point, "factor"), pp + ".factor") ?? throw new ArgumentException($"{pp}.factor is required"));
                    }).ToList());
                default:
                    throw new ArgumentException($"{path}.type '{type}' is not one of linear, power, logarithmic, interpolated, constant, breakpoints, expression");
            }
        }

        private static FontScalePolicy ReadFontScale(object value, string path)
        {
            if (value is string s && s == "follow") return FontScalePolicy.Follow;
            if (value is string s2 && s2 == "ignore") return FontScalePolicy.Ignore;
            if (value is Dictionary<string, object?> m)
                return FontScalePolicy.Clamp(Num(Get(m, "max"), path + ".max") ?? throw new ArgumentException($"{path}.max is required"));
            throw new ArgumentException($"{path} must be \"follow\", \"ignore\" or {{\"max\": n}}");
        }

        private static ScreenCondition ReadCondition(Dictionary<string, object?> obj, string path)
        {
            (float?, float?) Range(string key)
            {
                var value = Get(obj, key);
                if (value == null) return (null, null);
                if (!(value is Dictionary<string, object?> r)) throw new ArgumentException($"{path}.{key} must be an object with min/max");
                return (Num(Get(r, "min"), $"{path}.{key}.min"), Num(Get(r, "max"), $"{path}.{key}.max"));
            }

            IEnumerable<DeviceClass>? devices;
            switch (Get(obj, "device"))
            {
                case null: devices = null; break;
                case string d: devices = new[] { CustomNames.DeviceFromJson(d) }; break;
                case List<object?> l:
                    devices = l.Select(x => CustomNames.DeviceFromJson(x as string ?? throw new ArgumentException($"{path}.device entries must be strings"))).ToList();
                    break;
                default: throw new ArgumentException($"{path}.device must be a string or array");
            }
            bool? landscape;
            switch (Get(obj, "orientation") as string)
            {
                case null: landscape = null; break;
                case "portrait": landscape = false; break;
                case "landscape": landscape = true; break;
                case var o: throw new ArgumentException($"{path}.orientation '{o}' must be portrait or landscape");
            }
            var sw = Range("smallestWidth");
            var w = Range("width");
            var h = Range("height");
            var ar = Range("aspectRatio");
            return new ScreenCondition(devices, landscape, sw.Item1, sw.Item2, w.Item1, w.Item2, h.Item1, h.Item2,
                ar.Item1, ar.Item2, Get(obj, "multiWindow") as bool?);
        }

        private static float? Num(object? v, string path)
        {
            switch (v)
            {
                case null: return null;
                case double d:
                    var f = (float)d;
                    if (!float.IsFinite(f)) throw new ArgumentException($"{path} must be finite");
                    return f;
                default: throw new ArgumentException($"{path} must be a number");
            }
        }

        private static List<object?> List(object? v, string path) =>
            v as List<object?> ?? throw new ArgumentException($"{path} must be an array");

        // ── encoding ────────────────────────────────────────────────────────

        private static void WriteStrategy(StringBuilder sb, string id, ScalingConfig c)
        {
            sb.Append("{\"id\":").Append(Quote(id));
            WritePatchFields(sb, new ScalingPatch(c.Reference, c.Axis, c.Curve, c.Multiplier, c.MinFactor, c.MaxFactor,
                c.MinValue, c.MaxValue, c.AspectRatio, c.Sensitivity, c.FontScale, c.IgnoreMultiWindow, c.Transform, c.Formula), defaults: true);
            if (c.Rules.Count > 0)
            {
                sb.Append(",\"rules\":[");
                for (var i = 0; i < c.Rules.Count; i++)
                {
                    var rule = c.Rules[i];
                    if (i > 0) sb.Append(',');
                    sb.Append('{');
                    if (rule.Name != null) sb.Append("\"name\":").Append(Quote(rule.Name)).Append(',');
                    sb.Append("\"when\":");
                    WriteCondition(sb, rule.Condition);
                    sb.Append(",\"set\":{");
                    var start = sb.Length;
                    WritePatchFields(sb, rule.Patch, defaults: false);
                    if (sb.Length > start && sb[start] == ',') sb.Remove(start, 1);
                    sb.Append("}}");
                }
                sb.Append(']');
            }
            sb.Append('}');
        }

        private static void WritePatchFields(StringBuilder sb, ScalingPatch p, bool defaults)
        {
            if (p.Transform != null) throw new InvalidOperationException("ValueTransform lambdas cannot be encoded to JSON");
            if (p.Reference != null && (!defaults || !p.Reference.Equals(ReferenceScreen.Default)))
                sb.Append(",\"reference\":{\"width\":").Append(N(p.Reference.Width)).Append(",\"height\":").Append(N(p.Reference.Height)).Append('}');
            if (p.Axis.HasValue && (!defaults || p.Axis.Value != ScalingAxis.SmallestWidth))
                sb.Append(",\"axis\":").Append(Quote(p.Axis.Value.JsonName()));
            if (p.Curve != null) { sb.Append(",\"curve\":"); WriteCurve(sb, p.Curve); }
            if (p.Multiplier.HasValue && (!defaults || p.Multiplier.Value != 1f)) sb.Append(",\"multiplier\":").Append(N(p.Multiplier.Value));
            if (p.MinFactor.HasValue || p.MaxFactor.HasValue) sb.Append(",\"clamp\":").Append(Range(p.MinFactor, p.MaxFactor));
            if (p.MinValue.HasValue || p.MaxValue.HasValue) sb.Append(",\"valueClamp\":").Append(Range(p.MinValue, p.MaxValue));
            if (p.AspectRatio.HasValue && (!defaults || p.AspectRatio.Value || p.Sensitivity.HasValue))
            {
                sb.Append(",\"aspectRatio\":{\"enabled\":").Append(p.AspectRatio.Value ? "true" : "false");
                if (p.Sensitivity.HasValue) sb.Append(",\"sensitivity\":").Append(N(p.Sensitivity.Value));
                sb.Append('}');
            }
            if (p.FontScale != null && (!defaults || p.FontScale != FontScalePolicy.Follow))
            {
                sb.Append(",\"fontScale\":");
                if (p.FontScale == FontScalePolicy.Follow) sb.Append("\"follow\"");
                else if (p.FontScale == FontScalePolicy.Ignore) sb.Append("\"ignore\"");
                else sb.Append("{\"max\":").Append(N(((ClampPolicy)p.FontScale).Max)).Append('}');
            }
            if (p.IgnoreMultiWindow.HasValue && (!defaults || p.IgnoreMultiWindow.Value))
                sb.Append(",\"ignoreMultiWindow\":").Append(p.IgnoreMultiWindow.Value ? "true" : "false");
            if (p.Formula != null) sb.Append(",\"formula\":").Append(Quote(p.Formula.Expression.Source));
        }

        private static void WriteCurve(StringBuilder sb, ScalingCurve c)
        {
            switch (c)
            {
                case PowerCurve p: sb.Append("{\"type\":\"power\",\"exponent\":").Append(N(p.Exponent)).Append('}'); break;
                case LogarithmicCurve l: sb.Append("{\"type\":\"logarithmic\",\"k\":").Append(N(l.K)).Append('}'); break;
                case InterpolatedCurve t: sb.Append("{\"type\":\"interpolated\",\"t\":").Append(N(t.T)).Append('}'); break;
                case ConstantCurve k: sb.Append("{\"type\":\"constant\",\"factor\":").Append(N(k.Factor)).Append('}'); break;
                case BreakpointsCurve b:
                    sb.Append("{\"type\":\"breakpoints\",\"points\":[")
                      .Append(string.Join(",", b.Points.Select(x => "{\"at\":" + N(x.At) + ",\"factor\":" + N(x.Factor) + "}")))
                      .Append("]}");
                    break;
                case ExpressionCurve x: sb.Append("{\"type\":\"expression\",\"expression\":").Append(Quote(x.Body.Source)).Append('}'); break;
                case CustomCurve _: throw new InvalidOperationException("Custom curve lambdas cannot be encoded to JSON");
                default: sb.Append("{\"type\":\"linear\"}"); break;
            }
        }

        private static void WriteCondition(StringBuilder sb, ScreenCondition c)
        {
            var parts = new List<string>();
            if (c.Devices != null)
                parts.Add("\"device\":[" + string.Join(",", c.Devices.OrderBy(d => (int)d).Select(d => Quote(d.JsonName()))) + "]");
            if (c.Landscape.HasValue) parts.Add(c.Landscape.Value ? "\"orientation\":\"landscape\"" : "\"orientation\":\"portrait\"");
            if (c.MinSmallestWidth.HasValue || c.MaxSmallestWidth.HasValue) parts.Add("\"smallestWidth\":" + Range(c.MinSmallestWidth, c.MaxSmallestWidth));
            if (c.MinWidth.HasValue || c.MaxWidth.HasValue) parts.Add("\"width\":" + Range(c.MinWidth, c.MaxWidth));
            if (c.MinHeight.HasValue || c.MaxHeight.HasValue) parts.Add("\"height\":" + Range(c.MinHeight, c.MaxHeight));
            if (c.MinAspectRatio.HasValue || c.MaxAspectRatio.HasValue) parts.Add("\"aspectRatio\":" + Range(c.MinAspectRatio, c.MaxAspectRatio));
            if (c.MultiWindow.HasValue) parts.Add("\"multiWindow\":" + (c.MultiWindow.Value ? "true" : "false"));
            sb.Append('{').Append(string.Join(",", parts)).Append('}');
        }

        private static string Range(float? min, float? max)
        {
            var parts = new List<string>(2);
            if (min.HasValue) parts.Add("\"min\":" + N(min.Value));
            if (max.HasValue) parts.Add("\"max\":" + N(max.Value));
            return "{" + string.Join(",", parts) + "}";
        }

        private static string N(float v) => v.ToString("R", CultureInfo.InvariantCulture);

        internal static string Quote(string s)
        {
            var sb = new StringBuilder(s.Length + 2).Append('"');
            foreach (var ch in s)
            {
                switch (ch)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (ch < ' ') sb.Append("\\u").Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(ch);
                        break;
                }
            }
            return sb.Append('"').ToString();
        }
    }

    /// <summary>
    /// Minimal RFC 8259 parser → <c>Dictionary&lt;string, object?&gt;</c> / <c>List&lt;object?&gt;</c> /
    /// <c>string</c> / <c>double</c> / <c>bool</c> / <c>null</c>.
    /// </summary>
    public sealed class MiniJson
    {
        private readonly string _src;
        private int _i;

        public MiniJson(string src) { _src = src ?? throw new ArgumentNullException(nameof(src)); }

        public object? Parse()
        {
            var v = Value();
            Ws();
            if (_i != _src.Length) Fail("Unexpected trailing content");
            return v;
        }

        private object? Value()
        {
            Ws();
            if (_i >= _src.Length) Fail("Unexpected end of JSON");
            var c = _src[_i];
            switch (c)
            {
                case '{': return Obj();
                case '[': return Arr();
                case '"': return Str();
                case 't': return Lit("true", true);
                case 'f': return Lit("false", false);
                case 'n': return Lit("null", null);
                default:
                    if (c == '-' || char.IsDigit(c)) return Num();
                    Fail($"Unexpected character '{c}'");
                    return null;
            }
        }

        private Dictionary<string, object?> Obj()
        {
            var output = new Dictionary<string, object?>();
            _i++;
            Ws();
            if (Peek() == '}') { _i++; return output; }
            while (true)
            {
                Ws();
                if (Peek() != '"') Fail("Expected a string key");
                var k = Str();
                Ws();
                Expect(':');
                output[k] = Value();
                Ws();
                switch (Peek())
                {
                    case ',': _i++; break;
                    case '}': _i++; return output;
                    default: Fail("Expected ',' or '}'"); break;
                }
            }
        }

        private List<object?> Arr()
        {
            var output = new List<object?>();
            _i++;
            Ws();
            if (Peek() == ']') { _i++; return output; }
            while (true)
            {
                output.Add(Value());
                Ws();
                switch (Peek())
                {
                    case ',': _i++; break;
                    case ']': _i++; return output;
                    default: Fail("Expected ',' or ']'"); break;
                }
            }
        }

        private string Str()
        {
            _i++;
            var sb = new StringBuilder();
            while (true)
            {
                if (_i >= _src.Length) Fail("Unterminated string");
                var c = _src[_i++];
                if (c == '"') return sb.ToString();
                if (c != '\\') { sb.Append(c); continue; }
                if (_i >= _src.Length) Fail("Unterminated escape");
                var e = _src[_i++];
                switch (e)
                {
                    case '"': case '\\': case '/': sb.Append(e); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (_i + 4 > _src.Length) Fail("Bad unicode escape");
                        sb.Append((char)int.Parse(_src.Substring(_i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                        _i += 4;
                        break;
                    default: Fail($"Bad escape '\\{e}'"); break;
                }
            }
        }

        private double Num()
        {
            var start = _i;
            if (Peek() == '-') _i++;
            while (_i < _src.Length && (char.IsDigit(_src[_i]) || ".eE+-".IndexOf(_src[_i]) >= 0)) _i++;
            var text = _src.Substring(start, _i - start);
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) Fail($"Bad number '{text}'");
            return d;
        }

        private object? Lit(string word, object? v)
        {
            if (string.CompareOrdinal(_src, _i, word, 0, word.Length) != 0) Fail($"Expected '{word}'");
            _i += word.Length;
            return v;
        }

        private void Ws()
        {
            while (_i < _src.Length && char.IsWhiteSpace(_src[_i])) _i++;
        }

        private char Peek() => _i < _src.Length ? _src[_i] : '\0';

        private void Expect(char c)
        {
            if (Peek() != c) Fail($"Expected '{c}'");
            _i++;
        }

        private void Fail(string msg) => throw new ArgumentException($"Invalid JSON at {_i}: {msg}");
    }
}
