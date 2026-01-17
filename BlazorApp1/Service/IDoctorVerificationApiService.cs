using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public interface IDoctorVerificationApiService
    {
        Task<DoctorVerificationStatusDto?> GetStatusAsync(string doctorMongoId);
        Task UploadCertificateAsync(string doctorMongoId, byte[] fileBytes, string fileName, string contentType);
        string GetCertificateDownloadUrl(string doctorMongoId);
    }

    public class DoctorVerificationStatusDto
    {
        public string? Id { get; set; }
        public string? VerificationStatus { get; set; }
        public string? CertificateFileName { get; set; }
        public string? ReviewNotes { get; set; }
    }
}
