using System.Net.Http.Json;
using Model;

namespace BlazorApp1.Service
{
    public class UService : IUService
    {
        private readonly HttpClient _http;

        public UService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _http.GetFromJsonAsync<List<User>>("api/users")
                   ?? new List<User>();
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            var user = await _http.GetFromJsonAsync<User>($"api/users/{id}");
            if (user is null)
                throw new InvalidOperationException($"User not found for id: {id}");
            return user;
        }

        // ✅ matches IUService: AddUserAsync
        public async Task AddUserAsync(User user)
        {
            var res = await _http.PostAsJsonAsync("api/users", user);
            res.EnsureSuccessStatusCode();
        }

        public async Task UpdateUserAsync(string id, User user)
        {
            var res = await _http.PutAsJsonAsync($"api/users/{id}", user);
            res.EnsureSuccessStatusCode();
        }

        public async Task DeleteUserAsync(string id)
        {
            var res = await _http.DeleteAsync($"api/users/{id}");
            res.EnsureSuccessStatusCode();
        }

        // ✅ needed by your layout/pages
        public async Task<User?> GetUserByClientIdAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return null;

            try
            {
                return await _http.GetFromJsonAsync<User>($"api/users/by-client/{clientId}");
            }
            catch
            {
                return null;
            }
        }

        // ✅ needed by IUService interface
        public async Task UpdateUserByClientIdAsync(string clientId, User user)
        {
            var res = await _http.PutAsJsonAsync($"api/users/by-client/{clientId}", user);
            res.EnsureSuccessStatusCode();
        }
    }
}
