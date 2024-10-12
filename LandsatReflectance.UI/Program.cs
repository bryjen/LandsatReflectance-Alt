using System.Text.Encodings.Web;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LandsatReflectance.UI;
using LandsatReflectance.UI.Services;
using LandsatReflectance.UI.Services.Api;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(_ =>
{
    var jsonSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // To prevent escaping of '&'
    };
    return jsonSerializerOptions;
});

builder.Services.AddScoped(sp =>
{
    const string proxyServerBaseUri = "https://localhost:7280/";
    var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(proxyServerBaseUri);

    return httpClient;
});

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CurrentUserService>();

builder.Services.AddMudServices();

builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
