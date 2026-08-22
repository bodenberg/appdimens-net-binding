using AppDimens.Maui.BrowserDemo;
using AppDimens.Maui.Responsive;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Generated Android-parity bucket tables read straight from disk.
static string? FindGeneratedDir(string contentRoot)
{
    string[] candidates =
    [
        // publish layout: wwwroot carries a synced copy
        Path.Combine(contentRoot, "wwwroot", "Generated"),
        // dev layout: two levels up to the library root
        Path.GetFullPath(Path.Combine(contentRoot, "..", "..", "..",
            "src", "AppDimens.Maui.Resources", "Generated")),
    ];
    return candidates.FirstOrDefault(c =>
        Directory.Exists(c) && File.Exists(Path.Combine(c, "buckets.json")));
}

var generatedDir = FindGeneratedDir(builder.Environment.ContentRootPath);
if (generatedDir != null)
{
    var registry = BucketRegistry.LoadFromGenerated(generatedDir);
    builder.Services.AddSingleton(registry);
}
builder.Services.AddSingleton(sp => new BrowserDimensEngine(
    sp.GetService<BucketRegistry>()));

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
