using AppDimens.Maui.BrowserDemo;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var http = sp.GetRequiredService<HttpClient>();
    http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    return http;
});
builder.Services.AddSingleton<BrowserDimensEngine>();

await builder.Build().RunAsync();
