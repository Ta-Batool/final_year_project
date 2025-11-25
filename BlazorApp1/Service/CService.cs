using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        // Get client by email (nullable)
        public async Task<Client?> GetClientByEmailAsync(string email)
        {
            try
            {
                return await _http.GetFromJsonAsync<Client>($"api/clients/{email}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<List<Client>> GetAllClientsAsync()
        {
            return await _http.GetFromJsonAsync<List<Client>>("api/clients");
        }

        public async Task AddClientAsync(Client client)
        {
            var response = await _http.PostAsJsonAsync("api/clients", client);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }

        public async Task UpdateClientByEmailAsync(string email, Client client)
        {
            var response = await _http.PutAsJsonAsync($"api/clients/{email}", client);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }

        public async Task DeleteClientAsync(string email)
        {
            var response = await _http.DeleteAsync($"api/clients/{email}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }
    }
}
