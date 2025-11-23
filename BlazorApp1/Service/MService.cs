using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public class MService : IMService
    {
        private readonly HttpClient _http;

        public MService(HttpClient http)
        {
            _http = http;
        }

        // ✅ Get all messages
        public async Task<List<Message>> GetAllMessagesAsync()
        {
            return await _http.GetFromJsonAsync<List<Message>>("api/Message");
        }

        // ✅ Add new message
        public async Task AddMessageAsync(Message message)
        {
            var response = await _http.PostAsJsonAsync("api/Message", message);
            response.EnsureSuccessStatusCode();
        }

        // ✅ Get message by Id
        public async Task<Message> GetMessageByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<Message>($"api/Message/{id}");
        }

        // ✅ Update message
        public async Task UpdateMessageAsync(string id, Message message)
        {
            var response = await _http.PutAsJsonAsync($"api/Message/{id}", message);
            response.EnsureSuccessStatusCode();
        }

        // ✅ Delete message
        public async Task DeleteMessageAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Message/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
