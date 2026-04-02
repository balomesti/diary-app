using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using diary_app.Services;
using System.Net.Http;
using System;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<diary_app.App>("#app");

// Configure HttpClient with a delegating handler to attach JWT tokens
builder.Services.AddScoped<AuthTokenHandler>();
builder.Services.AddHttpClient("API", client => client.BaseAddress = new Uri("http://localhost:5105"))
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

// Add Services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DiaryService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddHttpClient("Speech", client => { });
builder.Services.AddScoped<SpeechService>();

await builder.Build().RunAsync();

// Delegating handler to add the auth token to outgoing requests
public class AuthTokenHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthTokenHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
