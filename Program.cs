using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using diary_app.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using System.Threading;
using System.Threading.Tasks;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<diary_app.App>("#app");

// 1. Register the token handler
builder.Services.AddScoped<AuthTokenHandler>();

// 2. Configure HttpClient to use the token handler
builder.Services.AddHttpClient("DiaryAPI", client => 
{
    client.BaseAddress = new Uri("http://localhost:5105");
})
.AddHttpMessageHandler<AuthTokenHandler>();

// 3. Supply the configured HttpClient by default
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("DiaryAPI"));

// Add Services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DiaryService>();

await builder.Build().RunAsync();

// The Handler that attaches the JWT token to every request
public class AuthTokenHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthTokenHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
