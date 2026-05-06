using System.Net;
using System.Net.Http.Json;
using Model;

namespace BlazorApp1.Service
{
    public class CService : ICService
    {
        private readonly HttpClient _http;

        public CService(HttpClient http)
        {
            _http = http;
        }

        public async Task<Client?> GetClientByEmailAsync(string email)
        {
            var safeEmail = Uri.EscapeDataString(email.Trim().ToLowerInvariant());

            var response = await _http.GetAsync($"api/clients/{safeEmail}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetClientByEmailAsync failed: {error}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Client>();
        }

        public async Task<List<Client>> GetAllClientsAsync()
        {
            return await _http.GetFromJsonAsync<List<Client>>("api/clients")
                   ?? new List<Client>();
        }

        public async Task AddClientAsync(Client client)
        {
            var response = await _http.PostAsJsonAsync("api/clients", client);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                if (error.Contains("Client already exists", StringComparison.OrdinalIgnoreCase))
                    return;

                throw new Exception($"API Error: {error}");
            }
        }

        public async Task UpdateClientByEmailAsync(string email, Client client)
        {
            var safeEmail = Uri.EscapeDataString(email.Trim().ToLowerInvariant());

            var response = await _http.PutAsJsonAsync($"api/clients/{safeEmail}", client);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }

        public async Task DeleteClientAsync(string email)
        {
            var safeEmail = Uri.EscapeDataString(email.Trim().ToLowerInvariant());

            var response = await _http.DeleteAsync($"api/clients/{safeEmail}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }
    }
}