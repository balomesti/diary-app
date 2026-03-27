using System.Net.Http.Json;

namespace diary_app.Services
{
    public class UserService
    {
        private readonly HttpClient _http;

        public UserService(HttpClient http)
        {
            _http = http;
        }

        public async Task<UserProfile?> GetProfileAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<UserProfile>("api/User/profile");
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateProfileAsync(UpdateProfileRequest request)
        {
            var response = await _http.PutAsJsonAsync("api/User/profile", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUsernameAsync(string username)
        {
            var response = await _http.PutAsJsonAsync("api/User/profile/username", new { Username = username });
            return response.IsSuccessStatusCode;
        }
    }

    public class UserProfile
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfileImg { get; set; }
        public string? UserBio { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? Username { get; set; }
        public string? ProfileImg { get; set; }
        public string? UserBio { get; set; }
    }
}