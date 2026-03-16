using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace diary_app.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
        _localStorage = localStorage;
    }

    public async Task<bool> Login(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        if (result == null) return false;

        await _localStorage.SetItemAsync("authToken", result.Token);
        ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticatedWithToken(result.Token);
        
        return true;
    }

    public async Task<bool> Register(string username, string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", new { Username = username, Email = email, PasswordHash = password });

        return response.IsSuccessStatusCode;
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
    }
}

public class LoginResult
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
