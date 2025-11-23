using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
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

        // 📨 Get full conversation between a user + doctor
        public async Task<List<Message>> GetConversationAsync(string userClientId, string doctorClientId)
        {
            var url = $"api/message/conversation?userClientId={Uri.EscapeDataString(userClientId)}&doctorClientId={Uri.EscapeDataString(doctorClientId)}";
            var result = await _http.GetFromJsonAsync<List<Message>>(url);
            return result ?? new List<Message>();
        }

        // 📨 Send a message
        public async Task<Message> SendMessageAsync(Message message)
        {
            var response = await _http.PostAsJsonAsync("api/message", message);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<Message>();
            if (created == null)
                throw new Exception("API returned empty message");

            return created;
        }

        // 👤 For USER: list of doctorClientIds this user has messaged
        public async Task<List<string>> GetDoctorsForUserAsync(string userClientId)
        {
            var url = $"api/message/user/{Uri.EscapeDataString(userClientId)}/doctors";
            var result = await _http.GetFromJsonAsync<List<string>>(url);
            return result ?? new List<string>();
        }

        // 👨‍⚕️ For DOCTOR: list of userClientIds this doctor has messaged
        public async Task<List<string>> GetUsersForDoctorAsync(string doctorClientId)
        {
            var url = $"api/message/doctor/{Uri.EscapeDataString(doctorClientId)}/users";
            var result = await _http.GetFromJsonAsync<List<string>>(url);
            return result ?? new List<string>();
        }
    }
}
