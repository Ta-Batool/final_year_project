using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            return await _http.GetFromJsonAsync<List<User>>("api/users");
        }

        public async Task AddUserAsync(User user)
        {
            var response = await _http.PostAsJsonAsync("api/users", user);
            response.EnsureSuccessStatusCode();
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<User>($"api/users/{id}");
        }

        public async Task UpdateUserAsync(string id, User user)
        {
            var response = await _http.PutAsJsonAsync($"api/users/{id}", user);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteUserAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/users/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<User?> GetUserByClientIdAsync(string clientId)
        {
            try
            {
                return await _http.GetFromJsonAsync<User>($"api/users/by-client/{clientId}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task UpdateUserByClientIdAsync(string clientId, User user)
        {
            var response = await _http.PutAsJsonAsync($"api/users/by-client/{clientId}", user);
            response.EnsureSuccessStatusCode();
        }
    }
}
