using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Presentation;
using Presentation.Services;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddRefitClient<IAuthService>()
    .ConfigureHttpClient(options =>
        options.BaseAddress = new Uri("https://localhost:7046"));

builder.Services.AddRefitClient<IWeatherForecastService>()
    .ConfigureHttpClient(options => 
        options.BaseAddress = new Uri("https://localhost:7046"))
    .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthHeaderHandler>());

builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddSingleton<TokenService>();

builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

await builder.Build().RunAsync();
