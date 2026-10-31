// AppDimens — Custom scaling strategies (portable engine). See CustomModel.cs.
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppDimens.Net.Custom
{
    /// <summary>Fields shared by the strategy root (<see cref="ScalingStrategyBuilder"/>) and rule blocks (<see cref="ScalingPatchBuilder"/>).</summary>
    public abstract class ScalingFields
    {
        private protected ScalingFields() { }

        public abstract ScalingAxis? Axis { get; set; }
        public abstract ScalingCurve? Curve { get; set; }
        public abstract float? Multiplier { get; set; }
        public abstract FontScalePolicy? FontScale { get; set; }
        public abstract bool? IgnoreMultiWindow { get; set; }

        private protected abstract void ApplyReference(ReferenceScreen reference);
        private protected abstract void ApplyFactorBounds(float? min, float? max);
        private protected abstract void ApplyValueBounds(float? min, float? max);
        private protected abstract void ApplyAspectRatio(bool enabled, float? sensitivity);
        private protected abstract void ApplyTransform(ValueTransform transform);
        private protected abstract void ApplyFormula(Formula formula);

        /// <summary>Design frame in dp (default 300×533).</summary>
        public void SetReference(float width = 300f, float height = 533f) => ApplyReference(new ReferenceScreen(width, height));

        /// <summary>Bounds for the <b>factor</b> (<c>Clamp(max: 1.5f)</c>).</summary>
        public void Clamp(float? min = null, float? max = null) => ApplyFactorBounds(min, max);

        /// <summary>Bounds for the <b>resulting dp</b> (value-dependent → cached).</summary>
        public void ClampValue(float? min = null, float? max = null) => ApplyValueBounds(min, max);

        /// <summary>Aspect-ratio correction <c>× (1 + k·ln(AR / 1.78))</c>.</summary>
        public void EnableAspectRatio(bool enabled = true, float? sensitivity = null) => ApplyAspectRatio(enabled, sensitivity);

        /// <summary>Per-value pure hook (value-dependent → cached).</summary>
        public void Transform(ValueTransform transform) => ApplyTransform(transform);

        /// <summary>
        /// Per-value formula as text (value-dependent → cached), e.g. <c>SetFormula("max(scaled, base)")</c> or a
        /// full replacement <c>SetFormula("base * pow(sw / 360, 0.7) + 2")</c>. See <see cref="ScalingExpression"/>.
        /// </summary>
        public void SetFormula(string source) => ApplyFormula(new Formula(source));

        /// <summary>Shortcut: multiply the factor.</summary>
        public void Multiply(float factor) => Multiplier = factor;

        public void FollowFontScale() => FontScale = FontScalePolicy.Follow;
        public void IgnoreFontScale() => FontScale = FontScalePolicy.Ignore;
        public void ClampFontScale(float max) => FontScale = FontScalePolicy.Clamp(max);
    }

    /// <summary>Block of a rule: every field is optional and overrides the strategy only when the rule matches.</summary>
    public sealed class ScalingPatchBuilder : ScalingFields
    {
        private ReferenceScreen? _reference;
        private float? _minFactor, _maxFactor, _minValue, _maxValue, _sensitivity;
        private bool? _aspect;
        private ValueTransform? _transform;
        private Formula? _formula;

        internal ScalingPatchBuilder() { }

        public override ScalingAxis? Axis { get; set; }
        public override ScalingCurve? Curve { get; set; }
        public override float? Multiplier { get; set; }
        public override FontScalePolicy? FontScale { get; set; }
        public override bool? IgnoreMultiWindow { get; set; }

        private protected override void ApplyReference(ReferenceScreen reference) => _reference = reference;
        private protected override void ApplyFactorBounds(float? min, float? max) { _minFactor = min; _maxFactor = max; }
        private protected override void ApplyValueBounds(float? min, float? max) { _minValue = min; _maxValue = max; }
        private protected override void ApplyAspectRatio(bool enabled, float? sensitivity) { _aspect = enabled; _sensitivity = sensitivity; }
        private protected override void ApplyTransform(ValueTransform transform) { _transform = transform; _formula = null; }
        private protected override void ApplyFormula(Formula formula) { _formula = formula; _transform = null; }

        internal ScalingPatch Build() => new ScalingPatch(
            _reference, Axis, Curve, Multiplier, _minFactor, _maxFactor, _minValue, _maxValue,
            _aspect, _sensitivity, FontScale, IgnoreMultiWindow, _transform, _formula);
    }

    /// <summary>Root builder used by <see cref="ScalingStrategy.Define"/> and <see cref="ScalingStrategy.Extend"/>.</summary>
    public sealed class ScalingStrategyBuilder : ScalingFields
    {
        private ScalingConfig _config;
        private readonly List<ScalingRule> _rules;

        public ScalingStrategyBuilder(ScalingConfig? baseConfig = null)
        {
            _config = baseConfig ?? ScalingConfig.Default;
            _rules = _config.Rules.ToList();
        }

        public override ScalingAxis? Axis
        {
            get => _config.Axis;
            set => _config = _config.With(axis: value ?? throw new ArgumentNullException(nameof(Axis)));
        }

        public override ScalingCurve? Curve
        {
            get => _config.Curve;
            set => _config = _config.With(curve: value ?? throw new ArgumentNullException(nameof(Curve)));
        }

        public override float? Multiplier
        {
            get => _config.Multiplier;
            set => _config = _config.With(multiplier: value ?? 1f);
        }

        public override FontScalePolicy? FontScale
        {
            get => _config.FontScale;
            set => _config = _config.With(fontScale: value ?? FontScalePolicy.Follow);
        }

        public override bool? IgnoreMultiWindow
        {
            get => _config.IgnoreMultiWindow;
            set => _config = _config.With(ignoreMultiWindow: value ?? false);
        }

        private protected override void ApplyReference(ReferenceScreen reference) => _config = _config.With(reference: reference);
        private protected override void ApplyFactorBounds(float? min, float? max) => _config = _config.WithFactorBounds(min, max);
        private protected override void ApplyValueBounds(float? min, float? max) => _config = _config.WithValueBounds(min, max);
        private protected override void ApplyAspectRatio(bool enabled, float? sensitivity) => _config = _config.WithAspectRatio(enabled, sensitivity);
        private protected override void ApplyTransform(ValueTransform transform) => _config = _config.WithTransform(transform);
        private protected override void ApplyFormula(Formula formula) => _config = _config.WithFormula(formula);

        /// <summary>Rule for one or more device classes.</summary>
        public void On(IEnumerable<DeviceClass> devices, Action<ScalingPatchBuilder> configure, string? name = null) =>
            Rule(new ScreenCondition(devices: devices), name, configure);

        /// <summary>Rule for a single device class.</summary>
        public void On(DeviceClass device, Action<ScalingPatchBuilder> configure, string? name = null) =>
            Rule(new ScreenCondition(devices: new[] { device }), name, configure);

        /// <summary>Rule for an arbitrary <paramref name="condition"/>.</summary>
        public void On(ScreenCondition condition, Action<ScalingPatchBuilder> configure, string? name = null) =>
            Rule(condition, name, configure);

        /// <summary>Rule by screen ranges (<c>min &lt;= v &lt; max</c>, dp).</summary>
        public void OnScreen(Action<ScalingPatchBuilder> configure,
            float? minSmallestWidth = null, float? maxSmallestWidth = null,
            float? minWidth = null, float? maxWidth = null,
            float? minHeight = null, float? maxHeight = null,
            float? minAspectRatio = null, float? maxAspectRatio = null,
            bool? landscape = null, IEnumerable<DeviceClass>? devices = null, bool? multiWindow = null,
            string? name = null) =>
            Rule(new ScreenCondition(devices, landscape, minSmallestWidth, maxSmallestWidth, minWidth, maxWidth,
                minHeight, maxHeight, minAspectRatio, maxAspectRatio, multiWindow), name, configure);

        public void Landscape(Action<ScalingPatchBuilder> configure) => Rule(new ScreenCondition(landscape: true), "landscape", configure);
        public void Portrait(Action<ScalingPatchBuilder> configure) => Rule(new ScreenCondition(landscape: false), "portrait", configure);
        public void MultiWindow(Action<ScalingPatchBuilder> configure) => Rule(new ScreenCondition(multiWindow: true), "multiWindow", configure);

        /// <summary>Shortcut for <c>sw &gt;= 600</c> (tablet-like windows, any UI mode).</summary>
        public void LargeScreens(Action<ScalingPatchBuilder> configure) =>
            Rule(new ScreenCondition(minSmallestWidth: CustomNames.TabletMinSmallestWidthDp), "largeScreens", configure);

        private void Rule(ScreenCondition condition, string? name, Action<ScalingPatchBuilder> configure)
        {
            var b = new ScalingPatchBuilder();
            configure(b);
            _rules.Add(new ScalingRule(condition, b.Build(), name));
        }

        internal ScalingConfig Build() => _config.WithRules(_rules);
    }
}
