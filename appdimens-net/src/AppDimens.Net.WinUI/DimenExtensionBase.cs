using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

namespace AppDimens.Net.WinUI;

/// <summary>Which WinUI type a dimension is converted to. <see cref="Auto"/> follows the target property.</summary>
public enum DimenTarget
{
    Auto,
    Double,
    Thickness,
    CornerRadius,
    GridLength,
}

/// <summary>
/// Base class of every <c>{ad:Xxx Value=16}</c> markup extension. The value is converted to the
/// property type (double, Thickness, CornerRadius, GridLength) and, when <see cref="Live"/> is true
/// (default), re-applied automatically after every window resize.
/// </summary>
public abstract class DimenExtensionBase : MarkupExtension
{
    /// <summary>Design value in dp (the number you would use on a 300dp-wide reference screen).</summary>
    public int Value { get; set; }

    /// <summary>When true (default) the value is updated on window resize.</summary>
    public bool Live { get; set; } = true;

    /// <summary>Forces the output type; <see cref="DimenTarget.Auto"/> detects it from the property.</summary>
    public DimenTarget As { get; set; } = DimenTarget.Auto;

    protected abstract DimenFunction Function { get; }

    // Before AppDimensWinUI.Initialize there is no window to scale against: keep the design value.
    internal static double Eval(DimenFunction function, int value) =>
        AppDimensAmbient.Current is null ? value : function.Evaluate(value);

    protected override object ProvideValue() => DimenValueConversion.Convert(Eval(Function, Value), null, As);

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Looks up the public static XxxProperty field of WinUI controls, which are never trimmed.")]
    protected override object ProvideValue(IXamlServiceProvider serviceProvider)
    {
        Type? targetType = null;
        if (serviceProvider?.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target &&
            target.TargetProperty is ProvideValueTargetProperty property)
        {
            targetType = property.Type;
            if (Live && target.TargetObject is DependencyObject owner && property.DeclaringType != null)
            {
                var dp = property.DeclaringType
                    .GetProperty(property.Name + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    ?.GetValue(null) as DependencyProperty;
                if (dp != null)
                    DimenLiveRegistry.Register(owner, dp, Function, Value, As, targetType);
            }
        }
        return DimenValueConversion.Convert(Eval(Function, Value), targetType, As);
    }
}

/// <summary>
/// Value converter for <c>{Binding}</c> / <c>{x:Bind}</c>: converts a bound number with
/// <see cref="Function"/> (or the function name passed as <c>ConverterParameter</c>).
/// </summary>
public sealed class DimenConverter : IValueConverter
{
    public DimenFunction Function { get; set; } = DimenFunction.Sdp;

    public DimenTarget As { get; set; } = DimenTarget.Auto;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var function = parameter switch
        {
            DimenFunction f => f,
            string s when Enum.TryParse<DimenFunction>(s, ignoreCase: true, out var parsed) => parsed,
            _ => Function,
        };
        var input = value switch
        {
            int i => i,
            IConvertible c => (int)Math.Round(c.ToDouble(CultureInfo.InvariantCulture)),
            _ => 0,
        };
        return DimenValueConversion.Convert(DimenExtensionBase.Eval(function, input), targetType, As);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

/// <summary>Weakly tracks XAML properties set by markup extensions and re-applies them on resize.</summary>
internal static class DimenLiveRegistry
{
    private sealed record Entry(WeakReference<DependencyObject> Owner, DependencyProperty Property,
        DimenFunction Function, int Value, DimenTarget As, Type? TargetType);

    private static readonly List<Entry> Entries = [];
    private static readonly object Sync = new();

    public static void Register(DependencyObject owner, DependencyProperty property, DimenFunction function,
        int value, DimenTarget As, Type? targetType)
    {
        lock (Sync)
            Entries.Add(new Entry(new WeakReference<DependencyObject>(owner), property, function, value, As, targetType));
    }

    public static void Reapply()
    {
        Entry[] snapshot;
        lock (Sync)
        {
            Entries.RemoveAll(e => !e.Owner.TryGetTarget(out _));
            snapshot = [.. Entries];
        }

        foreach (var entry in snapshot)
        {
            if (!entry.Owner.TryGetTarget(out var owner)) continue;
            var newValue = DimenValueConversion.Convert(DimenExtensionBase.Eval(entry.Function, entry.Value), entry.TargetType, entry.As);
            try
            {
                if (owner.DispatcherQueue is { HasThreadAccess: false } queue)
                    queue.TryEnqueue(() => owner.SetValue(entry.Property, newValue));
                else
                    owner.SetValue(entry.Property, newValue);
            }
            catch (Exception)
            {
                // Element torn down between snapshot and update: ignore.
            }
        }
    }
}

internal static class DimenValueConversion
{
    public static object Convert(double dp, Type? targetType, DimenTarget As)
    {
        switch (As)
        {
            case DimenTarget.Double: return dp;
            case DimenTarget.Thickness: return new Thickness(dp);
            case DimenTarget.CornerRadius: return new CornerRadius(dp);
            case DimenTarget.GridLength: return new GridLength(dp);
        }

        var t = targetType == null ? null : Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (t == null || t == typeof(object) || t == typeof(double)) return dp;
        if (t == typeof(Thickness)) return new Thickness(dp);
        if (t == typeof(CornerRadius)) return new CornerRadius(dp);
        if (t == typeof(GridLength)) return new GridLength(dp);
        if (t == typeof(float)) return (float)dp;
        if (t == typeof(int)) return (int)Math.Round(dp);
        if (t == typeof(string)) return dp.ToString(CultureInfo.InvariantCulture);
        return dp;
    }
}
