using System.Reflection;
using AppDimens.Maui.Core;
using Xunit;

namespace AppDimens.Maui.Tests;

/// <summary>
/// <see cref="DimenFunction"/> drives the WPF / Windows Forms / WinUI 3 markup extensions:
/// every member must map to the resolver method of the same name.
/// </summary>
public class DimenFunctionTests
{
    [Fact]
    public void Every_function_matches_resolver_method()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(411, 823, 2.0);
        r.CaptureBaseline();

        foreach (var function in Enum.GetValues<DimenFunction>())
        {
            var method = typeof(AppDimensResolver).GetMethod(function.ToString(), BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(method);
            var expected = (double)method!.Invoke(r, [16, InverterType.Default])!;
            Assert.Equal(expected, function.Evaluate(16), precision: 6);
        }
    }

    [Fact]
    public void Every_resolver_scaling_method_has_a_function()
    {
        var names = typeof(AppDimensResolver).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.ReturnType == typeof(double) && m.GetParameters() is [{ ParameterType: var p0 }, { ParameterType: var p1 }]
                        && p0 == typeof(int) && p1 == typeof(InverterType))
            .Select(m => m.Name)
            .ToHashSet();

        Assert.Equal(names.OrderBy(n => n), Enum.GetNames<DimenFunction>().OrderBy(n => n));
    }

    [Fact]
    public void Independent_flag_matches_suffix()
    {
        foreach (var function in Enum.GetValues<DimenFunction>())
        {
            var name = function.ToString();
            Assert.Equal(name.EndsWith('i') || name.EndsWith("ia"), function.IsIndependent());
        }
    }
}
