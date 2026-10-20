// AppDimens — Custom scaling strategies (portable engine). See CustomModel.cs.
#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AppDimens.Net.Custom
{
    /// <summary>
    /// Free-form formula written as text, so it can live in JSON and behaves the same on every
    /// platform. Booleans are numbers (true = 1, false = 0; non-zero is true).
    /// <para>Operators: <c>+ - * / % ^</c> (<c>^</c> is power, right-associative; <c>%</c> keeps the sign of
    /// the dividend), <c>== != &lt; &lt;= &gt; &gt;=</c>, <c>&amp;&amp; || !</c>, <c>cond ? a : b</c>, parentheses.</para>
    /// <para>Functions: <c>min(a, b, …)</c>, <c>max(a, b, …)</c>, <c>clamp(x, lo, hi)</c>, <c>pow</c>, <c>sqrt</c>,
    /// <c>ln</c>/<c>log</c> (natural), <c>log10</c>, <c>log2</c>, <c>exp</c>, <c>abs</c>, <c>sign</c>, <c>floor</c>,
    /// <c>ceil</c>, <c>round</c> (half up), <c>lerp(a, b, t)</c>, <c>step(edge, x)</c>, <c>smoothstep(e0, e1, x)</c>.</para>
    /// <para>Variables: <see cref="ScreenVariables"/>; curves add <c>ratio axis ref refWidth refHeight</c>;
    /// value formulas add <c>base scaled factor</c>.</para>
    /// </summary>
    public sealed class ScalingExpression : IEquatable<ScalingExpression>
    {
        public static readonly IReadOnlyCollection<string> ScreenVariables = new HashSet<string>
        {
            "sw", "width", "height", "shortSide", "longSide", "diagonal", "perimeter", "aspectRatio",
            "density", "fontScale", "landscape", "portrait", "multiWindow",
            "phone", "tablet", "desktop", "tv", "watch", "car", "vr", "pi", "e",
        };

        public static readonly IReadOnlyCollection<string> CurveVariables =
            new HashSet<string>(ScreenVariables.Concat(new[] { "ratio", "axis", "ref", "refWidth", "refHeight" }));

        public static readonly IReadOnlyCollection<string> FormulaVariables =
            new HashSet<string>(CurveVariables.Concat(new[] { "base", "scaled", "factor" }));

        private readonly Node _root;

        private ScalingExpression(string source, Node root)
        {
            Source = source;
            _root = root;
        }

        public string Source { get; }

        /// <summary>Parses <paramref name="source"/>; every identifier must be in <paramref name="allowed"/>.</summary>
        public static ScalingExpression Parse(string source, IReadOnlyCollection<string>? allowed = null) =>
            new ScalingExpression(source, new Parser(source ?? throw new ArgumentNullException(nameof(source)), allowed ?? FormulaVariables).ParseAll());

        /// <summary>Evaluates with a name → value lookup.</summary>
        public double Evaluate(Func<string, double> vars) => _root.Eval(vars);

        /// <summary>Variable lookup for <paramref name="screen"/> plus the strategy context.</summary>
        public static Func<string, double> Variables(ScreenSnapshot screen, float ratio, float axisDp, float refDp,
            ReferenceScreen reference, float baseValue = float.NaN, float scaled = float.NaN, float factor = float.NaN) => name =>
        {
            switch (name)
            {
                case "sw": return screen.SmallestWidthDp;
                case "width": return screen.WidthDp;
                case "height": return screen.HeightDp;
                case "shortSide": return screen.MinDp;
                case "longSide": return screen.MaxDp;
                case "diagonal": return screen.Read(ScalingAxis.Diagonal);
                case "perimeter": return screen.Read(ScalingAxis.Perimeter);
                case "aspectRatio": return screen.AspectRatio;
                case "density": return screen.Density;
                case "fontScale": return screen.FontScale;
                case "landscape": return screen.IsLandscape ? 1 : 0;
                case "portrait": return screen.IsLandscape ? 0 : 1;
                case "multiWindow": return screen.IsMultiWindow ? 1 : 0;
                case "phone": return screen.Device == DeviceClass.Phone ? 1 : 0;
                case "tablet": return screen.Device == DeviceClass.Tablet ? 1 : 0;
                case "desktop": return screen.Device == DeviceClass.Desktop ? 1 : 0;
                case "tv": return screen.Device == DeviceClass.Tv ? 1 : 0;
                case "watch": return screen.Device == DeviceClass.Watch ? 1 : 0;
                case "car": return screen.Device == DeviceClass.Car ? 1 : 0;
                case "vr": return screen.Device == DeviceClass.Vr ? 1 : 0;
                case "pi": return Math.PI;
                case "e": return Math.E;
                case "ratio": return ratio;
                case "axis": return axisDp;
                case "ref": return refDp;
                case "refWidth": return reference.Width;
                case "refHeight": return reference.Height;
                case "base": return baseValue;
                case "scaled": return scaled;
                case "factor": return factor;
                default: return double.NaN;
            }
        };

        public bool Equals(ScalingExpression? other) => other != null && other.Source == Source;
        public override bool Equals(object? obj) => Equals(obj as ScalingExpression);
        public override int GetHashCode() => Source.GetHashCode();
        public override string ToString() => Source;

        // ── AST ─────────────────────────────────────────────────────────────

        private abstract class Node
        {
            public abstract double Eval(Func<string, double> v);
        }

        private sealed class Num : Node
        {
            private readonly double _value;
            public Num(double value) { _value = value; }
            public override double Eval(Func<string, double> v) => _value;
        }

        private sealed class Var : Node
        {
            private readonly string _name;
            public Var(string name) { _name = name; }
            public override double Eval(Func<string, double> v) => v(_name);
        }

        private sealed class Unary : Node
        {
            private readonly char _op;
            private readonly Node _a;
            public Unary(char op, Node a) { _op = op; _a = a; }

            public override double Eval(Func<string, double> v)
            {
                var x = _a.Eval(v);
                return _op == '-' ? -x : (x != 0 ? 0 : 1);
            }
        }

        private sealed class Binary : Node
        {
            private readonly string _op;
            private readonly Node _a, _b;
            public Binary(string op, Node a, Node b) { _op = op; _a = a; _b = b; }

            public override double Eval(Func<string, double> v)
            {
                if (_op == "&&") return _a.Eval(v) != 0 && _b.Eval(v) != 0 ? 1 : 0;
                if (_op == "||") return _a.Eval(v) != 0 || _b.Eval(v) != 0 ? 1 : 0;
                var x = _a.Eval(v);
                var y = _b.Eval(v);
                switch (_op)
                {
                    case "+": return x + y;
                    case "-": return x - y;
                    case "*": return x * y;
                    case "/": return x / y;
                    case "%": return x % y; // truncated: keeps the sign of the dividend
                    case "^": return Math.Pow(x, y);
                    case "==": return x == y ? 1 : 0;
                    case "!=": return x != y ? 1 : 0;
                    case "<": return x < y ? 1 : 0;
                    case "<=": return x <= y ? 1 : 0;
                    case ">": return x > y ? 1 : 0;
                    default: return x >= y ? 1 : 0;
                }
            }
        }

        private sealed class Ternary : Node
        {
            private readonly Node _c, _a, _b;
            public Ternary(Node c, Node a, Node b) { _c = c; _a = a; _b = b; }
            public override double Eval(Func<string, double> v) => _c.Eval(v) != 0 ? _a.Eval(v) : _b.Eval(v);
        }

        private sealed class Call : Node
        {
            private readonly string _name;
            private readonly Node[] _args;
            public Call(string name, Node[] args) { _name = name; _args = args; }

            public override double Eval(Func<string, double> v)
            {
                var x = new double[_args.Length];
                for (var i = 0; i < x.Length; i++) x[i] = _args[i].Eval(v);
                switch (_name)
                {
                    case "min": return x.Min();
                    case "max": return x.Max();
                    case "clamp": return Math.Max(x[1], Math.Min(x[2], x[0]));
                    case "pow": return Math.Pow(x[0], x[1]);
                    case "sqrt": return Math.Sqrt(x[0]);
                    case "ln":
                    case "log": return Math.Log(x[0]);
                    case "log10": return Math.Log10(x[0]);
                    case "log2": return Math.Log(x[0]) / Math.Log(2);
                    case "exp": return Math.Exp(x[0]);
                    case "abs": return Math.Abs(x[0]);
                    case "sign": return double.IsNaN(x[0]) ? double.NaN : Math.Sign(x[0]);
                    case "floor": return Math.Floor(x[0]);
                    case "ceil": return Math.Ceiling(x[0]);
                    case "round": return Math.Floor(x[0] + 0.5);
                    case "lerp": return x[0] + (x[1] - x[0]) * x[2];
                    case "step": return x[1] < x[0] ? 0 : 1;
                    default:
                        var t = Math.Max(0, Math.Min(1, (x[2] - x[0]) / (x[1] - x[0])));
                        return t * t * (3 - 2 * t);
                }
            }
        }

        // ── Parser (recursive descent) ──────────────────────────────────────

        private sealed class Parser
        {
            private const int Many = int.MaxValue;

            private static readonly Dictionary<string, (int Min, int Max)> Arity = new Dictionary<string, (int, int)>
            {
                ["min"] = (1, Many), ["max"] = (1, Many), ["clamp"] = (3, 3), ["pow"] = (2, 2), ["sqrt"] = (1, 1),
                ["ln"] = (1, 1), ["log"] = (1, 1), ["log10"] = (1, 1), ["log2"] = (1, 1), ["exp"] = (1, 1),
                ["abs"] = (1, 1), ["sign"] = (1, 1), ["floor"] = (1, 1), ["ceil"] = (1, 1), ["round"] = (1, 1),
                ["lerp"] = (3, 3), ["step"] = (2, 2), ["smoothstep"] = (3, 3),
            };

            private static readonly string[] Comparisons = { "==", "!=", "<=", ">=", "<", ">" };

            private readonly string _src;
            private readonly IReadOnlyCollection<string> _allowed;
            private int _i;

            public Parser(string src, IReadOnlyCollection<string> allowed) { _src = src; _allowed = allowed; }

            public Node ParseAll()
            {
                if (string.IsNullOrWhiteSpace(_src)) throw Fail("empty expression");
                var n = Ternary();
                Ws();
                if (_i < _src.Length) throw Fail($"unexpected '{_src[_i]}'");
                return n;
            }

            private Node Ternary()
            {
                var c = Or();
                if (!Eat("?")) return c;
                var a = Ternary();
                if (!Eat(":")) throw Fail("expected ':'");
                return new Ternary(c, a, Ternary());
            }

            private Node Or()
            {
                var n = And();
                while (Eat("||")) n = new Binary("||", n, And());
                return n;
            }

            private Node And()
            {
                var n = Cmp();
                while (Eat("&&")) n = new Binary("&&", n, Cmp());
                return n;
            }

            private Node Cmp()
            {
                var n = Add();
                foreach (var op in Comparisons) if (Eat(op)) return new Binary(op, n, Add());
                return n;
            }

            private Node Add()
            {
                var n = Mul();
                while (true)
                {
                    if (Eat("+")) n = new Binary("+", n, Mul());
                    else if (Eat("-")) n = new Binary("-", n, Mul());
                    else return n;
                }
            }

            private Node Mul()
            {
                var n = UnaryNode();
                while (true)
                {
                    if (Eat("*")) n = new Binary("*", n, UnaryNode());
                    else if (Eat("/")) n = new Binary("/", n, UnaryNode());
                    else if (Eat("%")) n = new Binary("%", n, UnaryNode());
                    else return n;
                }
            }

            private Node UnaryNode()
            {
                if (Eat("-")) return new Unary('-', UnaryNode());
                if (Eat("+")) return UnaryNode();
                if (PeekIs("!") && !PeekIs("!=")) { Eat("!"); return new Unary('!', UnaryNode()); }
                return Power();
            }

            private Node Power()
            {
                var n = Primary();
                return Eat("^") ? new Binary("^", n, UnaryNode()) : n;
            }

            private Node Primary()
            {
                Ws();
                if (_i >= _src.Length) throw Fail("unexpected end");
                var c = _src[_i];
                if (c == '(')
                {
                    _i++;
                    var n = Ternary();
                    if (!Eat(")")) throw Fail("expected ')'");
                    return n;
                }
                if (char.IsDigit(c) || c == '.') return Number();
                if (char.IsLetter(c) || c == '_')
                {
                    var start = _i;
                    while (_i < _src.Length && (char.IsLetterOrDigit(_src[_i]) || _src[_i] == '_')) _i++;
                    var name = _src.Substring(start, _i - start);
                    if (Eat("(")) return CallNode(name, start);
                    if (name == "true") return new Num(1);
                    if (name == "false") return new Num(0);
                    if (!_allowed.Contains(name))
                    {
                        _i = start;
                        throw Fail($"unknown variable '{name}' (allowed: {string.Join(", ", _allowed.OrderBy(x => x, StringComparer.Ordinal))})");
                    }
                    return new Var(name);
                }
                throw Fail($"unexpected '{c}'");
            }

            private Node CallNode(string name, int start)
            {
                var args = new List<Node>();
                if (!Eat(")"))
                {
                    do args.Add(Ternary()); while (Eat(","));
                    if (!Eat(")")) throw Fail("expected ')'");
                }
                if (!Arity.TryGetValue(name, out var arity)) { _i = start; throw Fail($"unknown function '{name}'"); }
                if (args.Count < arity.Min || args.Count > arity.Max)
                {
                    _i = start;
                    throw Fail($"'{name}' takes {(arity.Min == arity.Max ? arity.Min.ToString(CultureInfo.InvariantCulture) : arity.Min + "+")} argument(s), got {args.Count}");
                }
                return new Call(name, args.ToArray());
            }

            private Node Number()
            {
                var start = _i;
                while (_i < _src.Length && (char.IsDigit(_src[_i]) || _src[_i] == '.')) _i++;
                if (_i < _src.Length && (_src[_i] == 'e' || _src[_i] == 'E'))
                {
                    var save = _i;
                    _i++;
                    if (_i < _src.Length && (_src[_i] == '+' || _src[_i] == '-')) _i++;
                    if (_i < _src.Length && char.IsDigit(_src[_i])) { while (_i < _src.Length && char.IsDigit(_src[_i])) _i++; }
                    else _i = save;
                }
                var text = _src.Substring(start, _i - start);
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) { _i = start; throw Fail($"bad number '{text}'"); }
                return new Num(d);
            }

            private void Ws()
            {
                while (_i < _src.Length && char.IsWhiteSpace(_src[_i])) _i++;
            }

            private bool PeekIs(string token)
            {
                Ws();
                return string.CompareOrdinal(_src, _i, token, 0, token.Length) == 0;
            }

            private bool Eat(string token)
            {
                if (!PeekIs(token)) return false;
                _i += token.Length;
                return true;
            }

            private ArgumentException Fail(string msg) => new ArgumentException($"Invalid expression at {_i}: {msg} in \"{_src}\"");
        }
    }

    /// <summary>Curve whose factor is a free-form <see cref="ScalingExpression"/> (JSON-serializable).</summary>
    public sealed class ExpressionCurve : ScalingCurve
    {
        internal ExpressionCurve(ScalingExpression body) { Body = body; }
        /// <summary>The parsed expression.</summary>
        public ScalingExpression Body { get; }
        public override bool Equals(object? obj) => obj is ExpressionCurve o && o.Body.Equals(Body);
        public override int GetHashCode() => Body.GetHashCode();
        public override string ToString() => $"Expression({Body.Source})";
    }

    /// <summary>
    /// Per-value formula written as text (<c>"formula": "max(scaled, base)"</c>); serializable to JSON.
    /// Inside a strategy it sees every curve variable plus <c>base</c>, <c>scaled</c> and <c>factor</c>.
    /// </summary>
    public sealed class Formula
    {
        public Formula(string source) { Expression = ScalingExpression.Parse(source, ScalingExpression.FormulaVariables); }
        public ScalingExpression Expression { get; }

        /// <summary>The formula as a <see cref="ValueTransform"/> (curve-context variables read as NaN).</summary>
        public ValueTransform AsTransform() => Transform;

        internal float Evaluate(ResolvedScaling r, float baseValue, float scaled) =>
            (float)Expression.Evaluate(ScalingExpression.Variables(r.Screen, r.ReferenceDp > 0f ? r.AxisDp / r.ReferenceDp : 1f,
                r.AxisDp, r.ReferenceDp, r.Config.Reference, baseValue, scaled, r.Factor));

        private float Transform(float baseValue, float scaled, ScreenSnapshot screen) =>
            (float)Expression.Evaluate(ScalingExpression.Variables(screen, float.NaN, float.NaN, float.NaN,
                ReferenceScreen.Default, baseValue, scaled, baseValue != 0f ? scaled / baseValue : float.NaN));

        public override bool Equals(object? obj) => obj is Formula o && o.Expression.Equals(Expression);
        public override int GetHashCode() => Expression.GetHashCode();
        public override string ToString() => $"Formula({Expression.Source})";
    }
}
