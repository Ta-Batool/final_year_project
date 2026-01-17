using API.Security;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminAuth _adminAuth;
        private readonly DoctorService _doctorService;
        private readonly IUserService _userService;

        public AdminController(AdminAuth adminAuth, DoctorService doctorService, IUserService userService)
        {
            _adminAuth = adminAuth;
            _doctorService = doctorService;
            _userService = userService;
        }

        private bool Guard() => _adminAuth.IsAdmin(Request);

        [HttpGet("doctors/pending")]
        public async Task<IActionResult> PendingDoctors()
        {
            if (!Guard()) return Unauthorized("Admin login required.");

            var docs = await _doctorService.GetByVerificationStatusAsync(DoctorVerificationStatus.PendingAdminApproval);
            return Ok(docs);
        }

        [HttpGet("patients")]
        public async Task<IActionResult> Patients()
        {
            if (!Guard()) return Unauthorized("Admin login required.");

            var patients = await _userService.GetAllAsync();
            return Ok(patients);
        }

        public class ReviewDto
        {
            public bool Approve { get; set; }
            public string? Notes { get; set; }
        }

        [HttpPost("doctors/{doctorId}/review")]
        public async Task<IActionResult> ReviewDoctor(string doctorId, [FromBody] ReviewDto dto)
        {
            if (!Guard()) return Unauthorized("Admin login required.");

            await _doctorService.ReviewDoctorAsync(doctorId, dto.Approve, "admin", dto.Notes);
            return Ok(new { message = dto.Approve ? "Approved" : "Rejected" });
        }
    }
}
