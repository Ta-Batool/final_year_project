using System.Net.Http;
using System.Net.Http.Json;

namespace BlazorApp1.Service
{
    public class DoctorVerificationApiService : IDoctorVerificationApiService
    {
        private readonly HttpClient _http;

        public DoctorVerificationApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<DoctorVerificationStatusDto?> GetStatusAsync(string doctorMongoId)
        {
            return await _http.GetFromJsonAsync<DoctorVerificationStatusDto>(
                $"api/doctor-verification/{doctorMongoId}/status");
        }

        public async Task UploadCertificateAsync(string doctorMongoId, byte[] fileBytes, string fileName, string contentType)
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            content.Add(fileContent, "file", fileName);

            var resp = await _http.PostAsync($"api/doctor-verification/{doctorMongoId}/upload-certificate", content);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                throw new Exception(err);
            }
        }

        public string GetCertificateDownloadUrl(string doctorMongoId)
            => _http.BaseAddress + $"api/doctor-verification/{doctorMongoId}/certificate";
    }
}
