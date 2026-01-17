using API.Services;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/doctor-verification")]
    public class DoctorVerificationController : ControllerBase
    {
        private readonly DoctorService _doctorService;

        public DoctorVerificationController(DoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet("{doctorId}/status")]
        public async Task<IActionResult> GetStatus(string doctorId)
        {
            var doctor = await _doctorService.GetByIdAsync(doctorId);
            if (doctor == null) return NotFound("Doctor not found.");

            return Ok(new
            {
                doctor.Id,
                doctor.VerificationStatus,
                doctor.ReviewNotes,
                doctor.CertificateFileName,
                doctor.CertificateUploadedAt
            });
        }

        [HttpPost("{doctorId}/upload")]
        [RequestSizeLimit(15_000_000)]
        public async Task<IActionResult> Upload(string doctorId, IFormFile file)
        {
            var doctor = await _doctorService.GetByIdAsync(doctorId);
            if (doctor == null) return NotFound("Doctor not found.");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var allowed = new[] { "application/pdf", "image/jpeg", "image/png" };
            if (!allowed.Contains(file.ContentType))
                return BadRequest("Only PDF/JPG/PNG allowed.");

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Certificates");
            Directory.CreateDirectory(dir);

            var safeName = $"{doctorId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
            var path = Path.Combine(dir, safeName);

            using (var fs = System.IO.File.Create(path))
            {
                await file.CopyToAsync(fs);
            }

            await _doctorService.UpdateCertificateAsync(doctorId, safeName, file.ContentType, path);

            return Ok(new { message = "Uploaded. Pending admin approval." });
        }

        [HttpGet("{doctorId}/certificate")]
        public async Task<IActionResult> DownloadCertificate(string doctorId)
        {
            var doctor = await _doctorService.GetByIdAsync(doctorId);
            if (doctor == null) return NotFound("Doctor not found.");

            if (string.IsNullOrWhiteSpace(doctor.CertificateStoragePath) ||
                !System.IO.File.Exists(doctor.CertificateStoragePath))
                return NotFound("Certificate not found.");

            var bytes = await System.IO.File.ReadAllBytesAsync(doctor.CertificateStoragePath);
            var ct = doctor.CertificateContentType ?? "application/octet-stream";
            var name = doctor.CertificateFileName ?? "certificate";

            return File(bytes, ct, name);
        }
    }
}
