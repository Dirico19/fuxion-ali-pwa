using Blazored.LocalStorage;
using FluentValidation;
using Fuxion.Ali.Contracts.Common;
using Fuxion.Ali.PWA;
using Fuxion.Ali.PWA.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.HostEnvironment.Environment}.json", optional: true, reloadOnChange: true);

var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl!) });
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
});
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddValidatorsFromAssemblyContaining<AseemblyMarker>();

await builder.Build().RunAsync();
