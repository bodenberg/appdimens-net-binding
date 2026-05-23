namespace AppDimens.Maui.Tests;

internal static class TestPaths
{
    public static string? GeneratedResources
    {
        get
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Generated"),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
                    "src", "AppDimens.Maui.Resources", "Generated")),
            };
            foreach (var dir in candidates)
            {
                if (File.Exists(Path.Combine(dir, "buckets.json")))
                    return dir;
            }
            return null;
        }
    }
}
