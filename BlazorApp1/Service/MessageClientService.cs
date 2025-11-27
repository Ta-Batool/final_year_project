using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public class MessageClientService
    {
        private readonly HttpClient _http;

        public MessageClientService(HttpClient http)
        {
            _http = http;
        }

        // ✅ 1-1 conversation (user–doctor) by their client IDs
        public async Task<List<Message>> GetConversationAsync(string userClientId, string doctorClientId)
        {
            var url =
                $"api/message/conversation?userClientId={Uri.EscapeDataString(userClientId)}" +
                $"&doctorClientId={Uri.EscapeDataString(doctorClientId)}";

            var result = await _http.GetFromJsonAsync<List<Message>>(url);
            return result ?? new List<Message>();
        }

        // ⭐ NEW: all messages in a group conversation
        public async Task<List<Message>> GetByConversationAsync(string conversationId)
        {
            var result = await _http.GetFromJsonAsync<List<Message>>(
                $"api/message/by-conversation/{Uri.EscapeDataString(conversationId)}");

            return result ?? new List<Message>();
        }

        // 🔹 Send plain text message (1-1 or group – just set fields on Message)
        public async Task<Message> SendMessageAsync(Message msg)
        {
            var response = await _http.PostAsJsonAsync("api/message", msg);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<Message>();
            if (created == null)
                throw new InvalidOperationException("API did not return created message.");

            return created;
        }

        // 🔹 Send message with attachment (image/file/audio)
        //     Used by Chat.razor, and can be reused by group chat if you pass conversationId.
        public async Task<Message> SendAttachmentAsync(MultipartFormDataContent content)
        {
            var response = await _http.PostAsync("api/message/with-attachment", content);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<Message>();
            if (created == null)
                throw new InvalidOperationException("API did not return created message with attachment.");

            return created;
        }

        public Uri? BaseAddress => _http.BaseAddress;
    }
}
