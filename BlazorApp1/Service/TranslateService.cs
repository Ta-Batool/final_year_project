using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BlazorApp1.Service
{
    public class TranslationService : ITranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public TranslationService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string[]> TranslateAsync(string targetLanguage, string[] texts)
        {
            var apiKey = _config["GoogleTranslate:ApiKey"]
                         ?? _config["GoogleTranslate__ApiKey"]; // env style

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Google Translate API key is missing.");

            var requestBody = new
            {
                q = texts,
                target = targetLanguage,
                format = "text",   // we translate plain text nodes
                source = "en"
            };

            using var response = await _httpClient.PostAsJsonAsync(
                $"https://translation.googleapis.com/language/translate/v2?key={apiKey}",
                requestBody);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GoogleTranslateResponse>();

            return result?.Data?.Translations?
                       .Select(t => System.Net.WebUtility.HtmlDecode(t.TranslatedText))
                       .ToArray()
                   ?? Array.Empty<string>();
        }

        private sealed class GoogleTranslateResponse
        {
            [JsonPropertyName("data")]
            public DataContainer? Data { get; set; }
        }

        private sealed class DataContainer
        {
            [JsonPropertyName("translations")]
            public List<TranslationItem>? Translations { get; set; }
        }

        private sealed class TranslationItem
        {
            [JsonPropertyName("translatedText")]
            public string TranslatedText { get; set; } = "";
        }
    }
}
