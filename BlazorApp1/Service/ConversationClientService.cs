using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public class ConversationClientService
    {
        private readonly HttpClient _http;

        public ConversationClientService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Conversation>> GetForClientAsync(string clientId)
        {
            var list = await _http.GetFromJsonAsync<List<Conversation>>($"api/conversation/for/{clientId}");
            return list ?? new List<Conversation>();
        }

        public async Task<Conversation?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<Conversation>($"api/conversation/{id}");
        }

        public async Task<Conversation> CreateAsync(Conversation conv)
        {
            var response = await _http.PostAsJsonAsync("api/conversation", conv);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<Conversation>();
            if (created == null) throw new InvalidOperationException("No conversation returned.");

            return created;
        }

        public async Task AddParticipantAsync(string conversationId, string clientId)
        {
            var url = $"api/conversation/{conversationId}/add-participant?clientId={Uri.EscapeDataString(clientId)}";
            var response = await _http.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
        }
    }
}
