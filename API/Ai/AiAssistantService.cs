using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace API.Ai
{
    public class AiAssistantService : IAiAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiAssistantService> _logger;

        private readonly string? _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;

        public AiAssistantService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AiAssistantService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // 🔹 Do NOT throw here, just log if missing
            _apiKey =
                Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ??
                configuration["OpenRouter:ApiKey"];

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogError("OPENROUTER_API_KEY is not configured. AI assistant will not work until it is set.");
            }

            _baseUrl =
                Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL") ??
                configuration["OpenRouter:BaseUrl"] ??
                "https://openrouter.ai/api/v1/chat/completions";

            _model =
                Environment.GetEnvironmentVariable("OPENROUTER_MODEL") ??
                configuration["OpenRouter:Model"] ??
                "mistral-small";
        }

        public Task<string> GetPatientReplyAsync(string userId, string message)
        {
            var context = $"PatientId: {userId}. (You can later add appointments/diet info here.)";

            var systemPrompt = """
                You are a friendly virtual health assistant for PATIENTS
                using an online doctor consultation and fitness app.

                - Answer calorie and nutrition questions briefly and clearly.
                - Provide basic exercise advice in simple steps.
                - Explain how the patient can use the app (contact doctor, book appointment).
                - NEVER give final diagnosis or prescribe medicine.
                - For serious or unclear symptoms, ALWAYS tell the patient to contact their doctor.

                Use simple, supportive language.
                """;

            return CallOpenRouterAsync(systemPrompt, context, message);
        }

        public Task<string> GetDoctorReplyAsync(string doctorId, string message)
        {
            var context = $"DoctorId: {doctorId}. (You can later add today’s schedule / patients here.)";

            var systemPrompt = """
                You are an AI assistant for DOCTORS using an online consultation
                and fitness management platform.

                - Help the doctor with productivity: summarise notes, draft messages, review schedules.
                - Use any provided context (appointments, patients) to answer clearly and concisely.
                - Provide only high-level medical information and ALWAYS remind that clinical judgment
                  and guidelines are required.
                - NEVER claim to replace professional medical decision-making or give definitive diagnoses.

                Answer in a concise, structured, professional tone.
                """;

            return CallOpenRouterAsync(systemPrompt, context, message);
        }

        private async Task<string> CallOpenRouterAsync(string systemPrompt, string context, string userMessage)
        {
            // 🔹 If key missing, return a clear message instead of crashing
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return "AI assistant is not configured on the server (missing OpenRouter API key). Please tell the admin.";
            }

            var payload = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = $"Context: {context}\n\nUser message: {userMessage}" }
                }
            };

            var json = JsonSerializer.Serialize(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Headers.Add("HTTP-Referer", "https://your-fyp-site.example"); // optional
            request.Headers.Add("X-Title", "Insha Tayyaba FYP Assistant");

            try
            {
                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);

                var content =
                    doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                if (string.IsNullOrWhiteSpace(content))
                    return "Sorry, I couldn't generate a response right now.";

                return content.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenRouter");
                return "Sorry, an error occurred while contacting the AI assistant.";
            }
        }
    }
}
