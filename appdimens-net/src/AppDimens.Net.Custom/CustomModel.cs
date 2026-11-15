// AppDimens — Custom scaling strategies (portable engine).
// The engine files (CustomModel, CustomEngine, CustomBuilder, CustomJson) carry no platform
// dependency: the same sources ship in AppDimens.Net.Custom and in the Unity/Godot port
// (appdimens-games/csharp). Keep them C# 8 compatible (no records, no file-scoped namespaces).
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppDimens.Net.Custom
{
    /// <summary>
    /// Screen measure a custom strategy scales against. The value read from the screen is divided
    /// by the same measure of the <see cref="ReferenceScreen"/> to obtain the ratio fed to the curve.
    /// </summary>
    public enum ScalingAxis
    {
        /// <summary>Smallest width (<c>sw</c>); reference = min(width, height).</summary>
        SmallestWidth,
        /// <summary>Current window width.</summary>
        Width,
        /// <summary>Current window height.</summary>
        Height,
        /// <summary>Shorter side of the window.</summary>
        Min,
        /// <summary>Longer side of the window.</summary>
        Max,
        /// <summary>√(w² + h²).</summary>
        Diagonal,
        /// <summary>w + h (half perimeter).</summary>
        Perimeter,
    }

    /// <summary>Device family used by rules. Detected from the window snapshot (UI mode + size).</summary>
    public enum DeviceClass
    {
        Phone,
        /// <summary>Normal UI mode with <c>sw &gt;= 600</c>.</summary>
        Tablet,
        /// <summary>Desk-docked / desktop windowing.</summary>
        Desktop,
        Tv,
        Watch,
        Car,
        Vr,
    }

    /// <summary>JSON names and parsing for the custom enums.</summary>
    public static class CustomNames
    {
        /// <summary><c>sw</c> (dp) from which a normal device is a tablet.</summary>
        public const float TabletMinSmallestWidthDp = 600f;

        private static readonly string[] AxisNames = { "smallestWidth", "width", "height", "min", "max", "diagonal", "perimeter" };
        private static readonly string[] DeviceNames = { "phone", "tablet", "desktop", "tv", "watch", "car", "vr" };

        public static string JsonName(this ScalingAxis axis) => AxisNames[(int)axis];
        public static string JsonName(this DeviceClass device) => DeviceNames[(int)device];

        public static ScalingAxis AxisFromJson(string name)
        {
            for (var i = 0; i < AxisNames.Length; i++)
                if (string.Equals(AxisNames[i], name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(((ScalingAxis)i).ToString(), name, StringComparison.OrdinalIgnoreCase))
                    return (ScalingAxis)i;
            throw new ArgumentException($"Unknown axis '{name}'. Expected one of [{string.Join(", ", AxisNames)}]");
        }

        public static DeviceClass DeviceFromJson(string name)
        {
            for (var i = 0; i < DeviceNames.Length; i++)
                if (string.Equals(DeviceNames[i], name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(((DeviceClass)i).ToString(), name, StringComparison.OrdinalIgnoreCase))
                    return (DeviceClass)i;
            throw new ArgumentException($"Unknown device '{name}'. Expected one of [{string.Join(", ", DeviceNames)}]");
        }

        /// <summary>Phone or tablet from the smallest width (normal UI mode).</summary>
        public static DeviceClass FromSmallestWidth(float smallestWidthDp) =>
            smallestWidthDp >= TabletMinSmallestWidthDp ? DeviceClass.Tablet : DeviceClass.Phone;

        /// <summary>
        /// Maps an Android <c>Configuration.uiMode</c> (type bits) and the smallest width to a class.
        /// Constants are inlined so the mapping stays platform-free.
        /// </summary>
        public static DeviceClass FromAndroidUiMode(int uiMode, float smallestWidthDp)
        {
            switch (uiMode & 0x0F)
            {
                case 2: return DeviceClass.Desktop; // UI_MODE_TYPE_DESK
                case 3: return DeviceClass.Car;     // UI_MODE_TYPE_CAR
                case 4: return DeviceClass.Tv;      // UI_MODE_TYPE_TELEVISION
                case 6: return DeviceClass.Watch;   // UI_MODE_TYPE_WATCH
                case 7: return DeviceClass.Vr;      // UI_MODE_TYPE_VR_HEADSET
                default: return FromSmallestWidth(smallestWidthDp);
            }
        }
    }

    /// <summary>
    /// Immutable, platform-free description of the window a value is resolved for.
    /// Every custom formula is a pure function of this snapshot.
    /// </summary>
    public sealed class ScreenSnapshot : IEquatable<ScreenSnapshot>
    {
        public ScreenSnapshot(float widthDp, float heightDp, float? smallestWidthDp = null, float density = 1f,
            float fontScale = 1f, bool? isLandscape = null, bool isMultiWindow = false, DeviceClass? device = null)
        {
            WidthDp = widthDp;
            HeightDp = heightDp;
            SmallestWidthDp = smallestWidthDp ?? Math.Min(widthDp, heightDp);
            Density = density;
            FontScale = fontScale;
            IsLandscape = isLandscape ?? widthDp > heightDp;
            IsMultiWindow = isMultiWindow;
            Device = device ?? CustomNames.FromSmallestWidth(SmallestWidthDp);
        }

        public float WidthDp { get; }
        public float HeightDp { get; }
        public float SmallestWidthDp { get; }
        public float Density { get; }
        public float FontScale { get; }
        public bool IsLandscape { get; }
        public bool IsMultiWindow { get; }
        public DeviceClass Device { get; }

        public float MinDp => Math.Min(WidthDp, HeightDp);
        public float MaxDp => Math.Max(WidthDp, HeightDp);

        /// <summary><c>max / min</c> (≥ 1).</summary>
        public float AspectRatio => MinDp > 0f ? MaxDp / MinDp : 1f;

        /// <summary>Reads <paramref name="axis"/> from this snapshot, in dp.</summary>
        public float Read(ScalingAxis axis)
        {
            switch (axis)
            {
                case ScalingAxis.SmallestWidth: return SmallestWidthDp;
                case ScalingAxis.Width: return WidthDp;
                case ScalingAxis.Height: return HeightDp;
                case ScalingAxis.Min: return MinDp;
                case ScalingAxis.Max: return MaxDp;
                case ScalingAxis.Diagonal: return (float)Math.Sqrt(MinDp * MinDp + MaxDp * MaxDp);
                default: return MinDp + MaxDp;
            }
        }

        public bool Equals(ScreenSnapshot? o) => o != null &&
            WidthDp.Equals(o.WidthDp) && HeightDp.Equals(o.HeightDp) && SmallestWidthDp.Equals(o.SmallestWidthDp) &&
            Density.Equals(o.Density) && FontScale.Equals(o.FontScale) && IsLandscape == o.IsLandscape &&
            IsMultiWindow == o.IsMultiWindow && Device == o.Device;

        public override bool Equals(object? obj) => Equals(obj as ScreenSnapshot);

        public override int GetHashCode()
        {
            unchecked
            {
                var h = WidthDp.GetHashCode();
                h = h * 31 + HeightDp.GetHashCode();
                h = h * 31 + SmallestWidthDp.GetHashCode();
                h = h * 31 + Density.GetHashCode();
                h = h * 31 + FontScale.GetHashCode();
                h = h * 31 + (IsLandscape ? 1 : 0);
                h = h * 31 + (IsMultiWindow ? 1 : 0);
                return h * 31 + (int)Device;
            }
        }
    }

    /// <summary>Design frame the values were drawn on (dp). Defaults to the AppDimens base 300×533.</summary>
    public sealed class ReferenceScreen : IEquatable<ReferenceScreen>
    {
        public static readonly ReferenceScreen Default = new ReferenceScreen();

        public ReferenceScreen(float width = 300f, float height = 533f)
        {
            if (!float.IsFinite(width) || width <= 0f) throw new ArgumentException("reference width must be > 0");
            if (!float.IsFinite(height) || height <= 0f) throw new ArgumentException("reference height must be > 0");
            Width = width;
            Height = height;
        }

        public float Width { get; }
        public float Height { get; }

        /// <summary>Same measure as <see cref="ScreenSnapshot.Read"/>, for the reference frame.</summary>
        public float Read(ScalingAxis axis)
        {
            switch (axis)
            {
                case ScalingAxis.SmallestWidth:
                case ScalingAxis.Min: return Math.Min(Width, Height);
                case ScalingAxis.Width: return Width;
                case ScalingAxis.Height: return Height;
                case ScalingAxis.Max: return Math.Max(Width, Height);
                case ScalingAxis.Diagonal: return (float)Math.Sqrt(Width * Width + Height * Height);
                default: return Width + Height;
            }
        }

        public bool Equals(ReferenceScreen? o) => o != null && Width.Equals(o.Width) && Height.Equals(o.Height);
        public override bool Equals(object? obj) => Equals(obj as ReferenceScreen);
        public override int GetHashCode() => unchecked(Width.GetHashCode() * 31 + Height.GetHashCode());
        public override string ToString() => $"Reference({Width}x{Height})";
    }

    /// <summary>Point of a breakpoints curve: at <see cref="At"/> dp on the axis, the factor is <see cref="Factor"/>.</summary>
    public readonly struct Breakpoint : IEquatable<Breakpoint>
    {
        public Breakpoint(float at, float factor)
        {
            if (!float.IsFinite(at) || at < 0f) throw new ArgumentException("breakpoint 'at' must be finite and >= 0");
            if (!float.IsFinite(factor)) throw new ArgumentException("breakpoint factor must be finite");
            At = at;
            Factor = factor;
        }

        public float At { get; }
        public float Factor { get; }
        public bool Equals(Breakpoint o) => At.Equals(o.At) && Factor.Equals(o.Factor);
        public override bool Equals(object? obj) => obj is Breakpoint b && Equals(b);
        public override int GetHashCode() => unchecked(At.GetHashCode() * 31 + Factor.GetHashCode());
        public override string ToString() => $"{At}->{Factor}";
    }

    /// <summary>Pure user formula: <c>(ratio, screen) =&gt; factor</c>. Runs once per snapshot (memoized).</summary>
    public delegate float CurveFunction(float ratio, ScreenSnapshot screen);

    /// <summary>
    /// Pure per-value hook applied after scaling: <c>(base, scaled, screen) =&gt; result</c> (dp).
    /// Using it makes the strategy value-dependent, so results go through the shared cache.
    /// </summary>
    public delegate float ValueTransform(float baseValue, float scaled, ScreenSnapshot screen);

    /// <summary>How the axis ratio <c>r = screen / reference</c> becomes a factor.</summary>
    public abstract class ScalingCurve
    {
        private protected ScalingCurve() { }

        /// <summary><c>r</c> — proportional (built-in <c>sdp</c>).</summary>
        public static readonly ScalingCurve Linear = new LinearCurve();

        /// <summary><c>r^exponent</c> — sublinear for <c>exponent &lt; 1</c> (built-in power uses 0.75).</summary>
        public static ScalingCurve Power(float exponent) => new PowerCurve(exponent);

        /// <summary><c>1 + k·ln(r)</c> — damped growth (built-in logarithmic uses 0.4).</summary>
        public static ScalingCurve Logarithmic(float k) => new LogarithmicCurve(k);

        /// <summary><c>1 + (r − 1)·t</c> — blend between fixed (0) and linear (1). Built-in uses 0.5.</summary>
        public static ScalingCurve Interpolated(float t) => new InterpolatedCurve(t);

        /// <summary>Fixed factor, ignores the screen (<c>Constant(1)</c> = plain dp).</summary>
        public static ScalingCurve Constant(float factor) => new ConstantCurve(factor);

        /// <summary>Piecewise-linear table over the <b>absolute</b> axis value (dp), clamped at the ends.</summary>
        public static ScalingCurve Breakpoints(IEnumerable<Breakpoint> points) => new BreakpointsCurve(points);

        /// <summary><c>Breakpoints((360, 1f), (600, 1.3f))</c> — <c>(axis dp, factor)</c> pairs.</summary>
        public static ScalingCurve Breakpoints(params (float at, float factor)[] points) =>
            new BreakpointsCurve(points.Select(p => new Breakpoint(p.at, p.factor)));

        /// <summary>
        /// Free formula written as text (<c>"pow(ratio, 0.8) * (tablet ? 0.9 : 1)"</c>) — see
        /// <see cref="ScalingExpression"/>. Serializable to JSON, identical on every platform.
        /// </summary>
        public static ScalingCurve Expression(string source) =>
            new ExpressionCurve(ScalingExpression.Parse(source, ScalingExpression.CurveVariables));

        /// <summary>Free pure formula as code — not serializable to JSON.</summary>
        public static ScalingCurve Custom(CurveFunction function) => new CustomCurve(function);

        internal static float Finite(float v, string name) =>
            float.IsFinite(v) ? v : throw new ArgumentException($"{name} must be finite");
    }

    public sealed class LinearCurve : ScalingCurve
    {
        internal LinearCurve() { }
        public override string ToString() => "Linear";
    }

    public sealed class PowerCurve : ScalingCurve
    {
        internal PowerCurve(float exponent) { Exponent = Finite(exponent, "exponent"); }
        public float Exponent { get; }
        public override bool Equals(object? obj) => obj is PowerCurve o && o.Exponent.Equals(Exponent);
        public override int GetHashCode() => Exponent.GetHashCode();
        public override string ToString() => $"Power({Exponent})";
    }

    public sealed class LogarithmicCurve : ScalingCurve
    {
        internal LogarithmicCurve(float k) { K = Finite(k, "k"); }
        public float K { get; }
        public override bool Equals(object? obj) => obj is LogarithmicCurve o && o.K.Equals(K);
        public override int GetHashCode() => K.GetHashCode();
        public override string ToString() => $"Logarithmic({K})";
    }

    public sealed class InterpolatedCurve : ScalingCurve
    {
        internal InterpolatedCurve(float t) { T = Finite(t, "t"); }
        public float T { get; }
        public override bool Equals(object? obj) => obj is InterpolatedCurve o && o.T.Equals(T);
        public override int GetHashCode() => T.GetHashCode();
        public override string ToString() => $"Interpolated({T})";
    }

    public sealed class ConstantCurve : ScalingCurve
    {
        internal ConstantCurve(float factor) { Factor = Finite(factor, "factor"); }
        public float Factor { get; }
        public override bool Equals(object? obj) => obj is ConstantCurve o && o.Factor.Equals(Factor);
        public override int GetHashCode() => Factor.GetHashCode();
        public override string ToString() => $"Constant({Factor})";
    }

    public sealed class BreakpointsCurve : ScalingCurve
    {
        internal BreakpointsCurve(IEnumerable<Breakpoint> points)
        {
            Points = points.OrderBy(p => p.At).ToArray();
            if (Points.Count == 0) throw new ArgumentException("breakpoints need at least one point");
        }

        public IReadOnlyList<Breakpoint> Points { get; }
        public override bool Equals(object? obj) => obj is BreakpointsCurve o && o.Points.SequenceEqual(Points);
        public override int GetHashCode() => Points.Aggregate(17, (h, p) => unchecked(h * 31 + p.GetHashCode()));
        public override string ToString() => $"Breakpoints([{string.Join(", ", Points)}])";
    }

    public sealed class CustomCurve : ScalingCurve
    {
        internal CustomCurve(CurveFunction function) { Function = function ?? throw new ArgumentNullException(nameof(function)); }
        public CurveFunction Function { get; }
        public override string ToString() => "Custom";
    }

    /// <summary>Font-scale handling for <c>csp</c>.</summary>
    public abstract class FontScalePolicy
    {
        private protected FontScalePolicy() { }

        /// <summary>Respect the user font scale (default, accessible).</summary>
        public static readonly FontScalePolicy Follow = new FollowPolicy();

        /// <summary>Ignore the user font scale (fixed-size labels, game HUD).</summary>
        public static readonly FontScalePolicy Ignore = new IgnorePolicy();

        /// <summary>Follow up to <paramref name="max"/> (e.g. 1.3).</summary>
        public static FontScalePolicy Clamp(float max) => new ClampPolicy(max);

        /// <summary>Multiplier that turns the system font scale into the effective one.</summary>
        public abstract float Effective(float systemFontScale);

        private sealed class FollowPolicy : FontScalePolicy
        {
            public override float Effective(float s) => s;
            public override string ToString() => "Follow";
        }

        private sealed class IgnorePolicy : FontScalePolicy
        {
            public override float Effective(float s) => 1f;
            public override string ToString() => "Ignore";
        }
    }

    /// <summary>Font-scale policy that follows the system up to <see cref="Max"/>.</summary>
    public sealed class ClampPolicy : FontScalePolicy
    {
        internal ClampPolicy(float max)
        {
            if (!float.IsFinite(max) || max <= 0f) throw new ArgumentException("fontScale max must be > 0");
            Max = max;
        }

        public float Max { get; }
        public override float Effective(float s) => Math.Min(s, Max);
        public override bool Equals(object? obj) => obj is ClampPolicy o && o.Max.Equals(Max);
        public override int GetHashCode() => Max.GetHashCode();
        public override string ToString() => $"Clamp({Max})";
    }

    /// <summary>
    /// When a <see cref="ScalingRule"/> applies. Every non-null field must match; ranges are
    /// <c>min &lt;= value &lt; max</c> (dp). An empty condition always matches.
    /// </summary>
    public sealed class ScreenCondition
    {
        public ScreenCondition(
            IEnumerable<DeviceClass>? devices = null, bool? landscape = null,
            float? minSmallestWidth = null, float? maxSmallestWidth = null,
            float? minWidth = null, float? maxWidth = null,
            float? minHeight = null, float? maxHeight = null,
            float? minAspectRatio = null, float? maxAspectRatio = null,
            bool? multiWindow = null)
        {
            Devices = devices == null ? null : new HashSet<DeviceClass>(devices);
            Landscape = landscape;
            MinSmallestWidth = minSmallestWidth; MaxSmallestWidth = maxSmallestWidth;
            MinWidth = minWidth; MaxWidth = maxWidth;
            MinHeight = minHeight; MaxHeight = maxHeight;
            MinAspectRatio = minAspectRatio; MaxAspectRatio = maxAspectRatio;
            MultiWindow = multiWindow;
        }

        public IReadOnlyCollection<DeviceClass>? Devices { get; }
        /// <summary><c>true</c> = landscape only, <c>false</c> = portrait only, <c>null</c> = any.</summary>
        public bool? Landscape { get; }
        public float? MinSmallestWidth { get; }
        public float? MaxSmallestWidth { get; }
        public float? MinWidth { get; }
        public float? MaxWidth { get; }
        public float? MinHeight { get; }
        public float? MaxHeight { get; }
        public float? MinAspectRatio { get; }
        public float? MaxAspectRatio { get; }
        public bool? MultiWindow { get; }

        public bool Matches(ScreenSnapshot screen)
        {
            if (Devices != null && !Devices.Contains(screen.Device)) return false;
            if (Landscape.HasValue && screen.IsLandscape != Landscape.Value) return false;
            if (!InRange(screen.SmallestWidthDp, MinSmallestWidth, MaxSmallestWidth)) return false;
            if (!InRange(screen.WidthDp, MinWidth, MaxWidth)) return false;
            if (!InRange(screen.HeightDp, MinHeight, MaxHeight)) return false;
            if (!InRange(screen.AspectRatio, MinAspectRatio, MaxAspectRatio)) return false;
            if (MultiWindow.HasValue && screen.IsMultiWindow != MultiWindow.Value) return false;
            return true;
        }

        private static bool InRange(float value, float? min, float? max) =>
            (!min.HasValue || value >= min.Value) && (!max.HasValue || value < max.Value);
    }

    /// <summary>Partial override applied by a rule. <c>null</c> keeps the current value.</summary>
    public sealed class ScalingPatch
    {
        public ScalingPatch(
            ReferenceScreen? reference = null, ScalingAxis? axis = null, ScalingCurve? curve = null,
            float? multiplier = null, float? minFactor = null, float? maxFactor = null,
            float? minValue = null, float? maxValue = null, bool? aspectRatio = null, float? sensitivity = null,
            FontScalePolicy? fontScale = null, bool? ignoreMultiWindow = null, ValueTransform? transform = null,
            Formula? formula = null)
        {
            Reference = reference; Axis = axis; Curve = curve; Multiplier = multiplier; Formula = formula;
            MinFactor = minFactor; MaxFactor = maxFactor; MinValue = minValue; MaxValue = maxValue;
            AspectRatio = aspectRatio; Sensitivity = sensitivity; FontScale = fontScale;
            IgnoreMultiWindow = ignoreMultiWindow; Transform = transform;
        }

        public ReferenceScreen? Reference { get; }
        public ScalingAxis? Axis { get; }
        public ScalingCurve? Curve { get; }
        public float? Multiplier { get; }
        public float? MinFactor { get; }
        public float? MaxFactor { get; }
        public float? MinValue { get; }
        public float? MaxValue { get; }
        public bool? AspectRatio { get; }
        public float? Sensitivity { get; }
        public FontScalePolicy? FontScale { get; }
        public bool? IgnoreMultiWindow { get; }
        public ValueTransform? Transform { get; }
        /// <summary>Per-value text formula (JSON-serializable). Setting it replaces <see cref="Transform"/>.</summary>
        public Formula? Formula { get; }

        // A proportional text formula (base * E) is screen-only; any other formula or transform is not.
        internal bool IsValueDependent =>
            MinValue.HasValue || MaxValue.HasValue || Transform != null || (Formula != null && !Formula.IsProportional);
    }

    /// <summary>
    /// When <see cref="Condition"/> matches, apply <see cref="Patch"/>. Matching rules apply in
    /// declaration order; a later rule overrides fields set by an earlier one.
    /// </summary>
    public sealed class ScalingRule
    {
        public ScalingRule(ScreenCondition condition, ScalingPatch patch, string? name = null)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Patch = patch ?? throw new ArgumentNullException(nameof(patch));
            Name = name;
        }

        public ScreenCondition Condition { get; }
        public ScalingPatch Patch { get; }
        public string? Name { get; }

        /// <summary>Rule for one or more device classes.</summary>
        public static ScalingRule On(IEnumerable<DeviceClass> devices, ScalingPatch patch, string? name = null) =>
            new ScalingRule(new ScreenCondition(devices: devices), patch, name);
    }

    /// <summary>
    /// Full, immutable definition of a custom strategy.
    /// <code>
    /// factor = clamp(curve(axis(screen) / axis(reference)) × multiplier × AR, minFactor, maxFactor)
    /// dp     = transform(clampValue(base × factor, minValue, maxValue))
    /// </code>
    /// </summary>
    public sealed class ScalingConfig
    {
        public static readonly ScalingConfig Default = new ScalingConfig();

        public ScalingConfig(
            ReferenceScreen? reference = null, ScalingAxis axis = ScalingAxis.SmallestWidth, ScalingCurve? curve = null,
            float multiplier = 1f, float? minFactor = null, float? maxFactor = null,
            float? minValue = null, float? maxValue = null, bool aspectRatio = false, float? sensitivity = null,
            FontScalePolicy? fontScale = null, bool ignoreMultiWindow = false, ValueTransform? transform = null,
            IEnumerable<ScalingRule>? rules = null, Formula? formula = null)
        {
            if (!float.IsFinite(multiplier)) throw new ArgumentException("multiplier must be finite");
            FiniteOrNull(minFactor, "minFactor");
            FiniteOrNull(maxFactor, "maxFactor");
            FiniteOrNull(minValue, "minValue");
            FiniteOrNull(maxValue, "maxValue");
            FiniteOrNull(sensitivity, "sensitivity");
            Reference = reference ?? ReferenceScreen.Default;
            Axis = axis;
            Curve = curve ?? ScalingCurve.Linear;
            Multiplier = multiplier;
            MinFactor = minFactor; MaxFactor = maxFactor;
            MinValue = minValue; MaxValue = maxValue;
            AspectRatio = aspectRatio;
            Sensitivity = sensitivity;
            FontScale = fontScale ?? FontScalePolicy.Follow;
            IgnoreMultiWindow = ignoreMultiWindow;
            Transform = formula != null ? null : transform;
            Formula = formula;
            Rules = rules?.ToArray() ?? Array.Empty<ScalingRule>();
        }

        public ReferenceScreen Reference { get; }
        public ScalingAxis Axis { get; }
        public ScalingCurve Curve { get; }
        public float Multiplier { get; }
        public float? MinFactor { get; }
        public float? MaxFactor { get; }
        public float? MinValue { get; }
        public float? MaxValue { get; }
        public bool AspectRatio { get; }
        public float? Sensitivity { get; }
        public FontScalePolicy FontScale { get; }
        /// <summary>In multi-window the value stays unscaled (only value bounds apply); formulas and transforms are skipped.</summary>
        public bool IgnoreMultiWindow { get; }
        /// <summary>Code-only per-value hook (not serializable).</summary>
        public ValueTransform? Transform { get; }
        /// <summary>Per-value text formula (JSON-serializable); takes the place of <see cref="Transform"/>.</summary>
        public Formula? Formula { get; }
        public IReadOnlyList<ScalingRule> Rules { get; }

        /// <summary>True when some path depends on the value itself (not only on the screen).</summary>
        public bool IsValueDependent =>
            MinValue.HasValue || MaxValue.HasValue || Transform != null || (Formula != null && !Formula.IsProportional) ||
            Rules.Any(r => r.Patch.IsValueDependent);

        /// <summary>
        /// Cross-field checks for a declared config (the base and each rule on top of it).
        /// Rules combined at runtime never throw: when bounds cross, the max bound wins.
        /// </summary>
        public ScalingConfig Validate()
        {
            if (MinFactor.HasValue && MaxFactor.HasValue && MinFactor.Value > MaxFactor.Value)
                throw new ArgumentException("minFactor must be <= maxFactor");
            if (MinValue.HasValue && MaxValue.HasValue && MinValue.Value > MaxValue.Value)
                throw new ArgumentException("minValue must be <= maxValue");
            // Value bounds clamp the magnitude, so they must not be negative.
            if (MinValue.HasValue && MinValue.Value < 0f) throw new ArgumentException("minValue must be >= 0");
            if (MaxValue.HasValue && MaxValue.Value < 0f) throw new ArgumentException("maxValue must be >= 0");
            foreach (var r in Rules) Apply(r.Patch).WithRules(Array.Empty<ScalingRule>()).Validate();
            return this;
        }

        /// <summary>Returns this config with <paramref name="patch"/> applied.</summary>
        public ScalingConfig Apply(ScalingPatch patch) => new ScalingConfig(
            patch.Reference ?? Reference, patch.Axis ?? Axis, patch.Curve ?? Curve,
            patch.Multiplier ?? Multiplier, patch.MinFactor ?? MinFactor, patch.MaxFactor ?? MaxFactor,
            patch.MinValue ?? MinValue, patch.MaxValue ?? MaxValue, patch.AspectRatio ?? AspectRatio,
            patch.Sensitivity ?? Sensitivity, patch.FontScale ?? FontScale,
            patch.IgnoreMultiWindow ?? IgnoreMultiWindow,
            patch.Formula != null ? null : patch.Transform ?? Transform, Rules,
            patch.Transform != null && patch.Formula == null ? null : patch.Formula ?? Formula);

        /// <summary>Copy with the given fields replaced (pass only what changes).</summary>
        public ScalingConfig With(
            ReferenceScreen? reference = null, ScalingAxis? axis = null, ScalingCurve? curve = null,
            float? multiplier = null, bool? aspectRatio = null, FontScalePolicy? fontScale = null,
            bool? ignoreMultiWindow = null) => new ScalingConfig(
            reference ?? Reference, axis ?? Axis, curve ?? Curve, multiplier ?? Multiplier,
            MinFactor, MaxFactor, MinValue, MaxValue, aspectRatio ?? AspectRatio, Sensitivity,
            fontScale ?? FontScale, ignoreMultiWindow ?? IgnoreMultiWindow, Transform, Rules, Formula);

        public ScalingConfig WithFactorBounds(float? min, float? max) => new ScalingConfig(
            Reference, Axis, Curve, Multiplier, min, max, MinValue, MaxValue, AspectRatio, Sensitivity,
            FontScale, IgnoreMultiWindow, Transform, Rules, Formula);

        public ScalingConfig WithValueBounds(float? min, float? max) => new ScalingConfig(
            Reference, Axis, Curve, Multiplier, MinFactor, MaxFactor, min, max, AspectRatio, Sensitivity,
            FontScale, IgnoreMultiWindow, Transform, Rules, Formula);

        public ScalingConfig WithAspectRatio(bool enabled, float? sensitivity) => new ScalingConfig(
            Reference, Axis, Curve, Multiplier, MinFactor, MaxFactor, MinValue, MaxValue, enabled, sensitivity,
            FontScale, IgnoreMultiWindow, Transform, Rules, Formula);

        public ScalingConfig WithTransform(ValueTransform? transform) => new ScalingConfig(
            Reference, Axis, Curve, Multiplier, MinFactor, MaxFactor, MinValue, MaxValue, AspectRatio, Sensitivity,
            FontScale, IgnoreMultiWindow, transform, Rules, null);

        public ScalingConfig WithFormula(Formula? formula) => new ScalingConfig(
            Reference, Axis, Curve, Multiplier, MinFactor, MaxFactor, MinValue, MaxValue, AspectRatio, Sensitivity,
            FontScale, IgnoreMultiWindow, null, Rules, formula);

        public ScalingConfig WithRules(IEnumerable<ScalingRule> rules) => new ScalingConfig(
            Reference, Axis, Curve, Multiplier, MinFactor, MaxFactor, MinValue, MaxValue, AspectRatio, Sensitivity,
            FontScale, IgnoreMultiWindow, Transform, rules, Formula);

        private static void FiniteOrNull(float? v, string name)
        {
            if (v.HasValue && !float.IsFinite(v.Value)) throw new ArgumentException($"{name} must be finite");
        }
    }
}
