using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace AppDimens.Net.Wpf;

/// <summary>Which WPF type a dimension is converted to. <see cref="Auto"/> follows the target property.</summary>
public enum DimenTarget
{
    Auto,
    Double,
    Thickness,
    CornerRadius,
    GridLength,
}

/// <summary>
/// Base class of every <c>{ad:Xxx 16}</c> markup extension. By default the value is
/// <b>live</b>: it is bound to the window size and recalculated automatically on resize.
/// Works for <c>double</c> properties (Width, Height, FontSize…) and converts to
/// <see cref="Thickness"/>, <see cref="CornerRadius"/> and <see cref="GridLength"/> automatically.
/// </summary>
public abstract class DimenExtensionBase : MarkupExtension
{
    /// <summary>Design value in dp (the number you would use on a 300dp-wide reference screen).</summary>
    [ConstructorArgument("value")]
    public int Value { get; set; }

    /// <summary>When true (default) the value updates on window resize.</summary>
    public bool Live { get; set; } = true;

    /// <summary>Forces the output type; <see cref="DimenTarget.Auto"/> detects it from the property.</summary>
    public DimenTarget As { get; set; } = DimenTarget.Auto;

    protected abstract DimenFunction Function { get; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        AppDimensWpf.EnsureInitialized();
        var target = serviceProvider?.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

        if (Live && CanBind(target))
        {
            var binding = new Binding(nameof(DimenLiveSource.Version))
            {
                Source = DimenLiveSource.Instance,
                Mode = BindingMode.OneWay,
                Converter = new DimenConverter { Function = Function, Value = Value, As = As },
            };
            return binding.ProvideValue(serviceProvider!);
        }

        return DimenValueConversion.Convert(Function.Evaluate(Value), TargetType(target), As);
    }

    private static bool CanBind(IProvideValueTarget? target)
    {
        if (target == null) return false;
        if (target.TargetProperty is DependencyProperty) return true;
        // Style setters, triggers and templates accept bindings as values.
        if (target.TargetObject is SetterBase) return true;
        return target.TargetObject?.GetType().FullName == "System.Windows.SharedDp";
    }

    private static Type? TargetType(IProvideValueTarget? target) => target?.TargetProperty switch
    {
        DependencyProperty dp => dp.PropertyType,
        PropertyInfo pi => pi.PropertyType,
        _ => null,
    };
}

/// <summary>Singleton whose <see cref="Version"/> changes on every resize; live bindings listen to it.</summary>
public sealed class DimenLiveSource : INotifyPropertyChanged
{
    public static DimenLiveSource Instance { get; } = new();

    private int _version;

    public int Version => _version;

    public event PropertyChangedEventHandler? PropertyChanged;

    internal void Notify()
    {
        Interlocked.Increment(ref _version);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Version)));
    }
}

/// <summary>
/// Value converter for bindings. Two ways to use it:
/// <list type="bullet">
/// <item>Fixed: <c>Converter={ad:DimenConverter Function=Sdp, Value=16}</c> (input ignored).</item>
/// <item>From data: bind a number and set <c>Function</c> (or pass it as <c>ConverterParameter</c>).</item>
/// </list>
/// </summary>
public sealed class DimenConverter : MarkupExtension, IValueConverter
{
    public DimenFunction Function { get; set; } = DimenFunction.Sdp;

    /// <summary>Fixed design value; when not set, the bound number is used instead.</summary>
    public int? Value { get; set; }

    public DimenTarget As { get; set; } = DimenTarget.Auto;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var function = parameter switch
        {
            DimenFunction f => f,
            string s when Enum.TryParse<DimenFunction>(s, ignoreCase: true, out var parsed) => parsed,
            _ => Function,
        };
        var input = Value ?? ToInt(value);
        return DimenValueConversion.Convert(function.Evaluate(input), targetType, As);
    }

    private static int ToInt(object? value) => value switch
    {
        null => 0,
        int i => i,
        IConvertible c => (int)Math.Round(c.ToDouble(CultureInfo.InvariantCulture)),
        _ => 0,
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
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

        var t = Nullable.GetUnderlyingType(targetType ?? typeof(object)) ?? targetType;
        if (t == null || t == typeof(object) || t == typeof(double)) return dp;
        if (t == typeof(Thickness)) return new Thickness(dp);
        if (t == typeof(CornerRadius)) return new CornerRadius(dp);
        if (t == typeof(GridLength)) return new GridLength(dp);
        if (t == typeof(float)) return (float)dp;
        if (t == typeof(int)) return (int)Math.Round(dp);
        if (t == typeof(string)) return dp.ToString(CultureInfo.InvariantCulture);
        if (t == typeof(Size)) return new Size(dp, dp);
        if (t == typeof(Point)) return new Point(dp, dp);
        return dp;
    }
}
