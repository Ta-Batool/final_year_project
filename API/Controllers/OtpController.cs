using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/otp")]
    public class OtpController : ControllerBase
    {
        private readonly OtpService _otp;

        public OtpController(OtpService otp)
        {
            _otp = otp;
        }

        public record SendOtpRequest(string Phone);
        public record VerifyOtpRequest(string Phone, string Code);

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendOtpRequest req)
        {
            var (ok, message, otpForDev) = await _otp.SendAsync(req.Phone);

            if (!ok) return BadRequest(message);

            // In dev you may return OTP for testing
            if (!string.IsNullOrWhiteSpace(otpForDev))
                return Ok(new { message, otp = otpForDev });

            return Ok(new { message });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyOtpRequest req)
        {
            var (ok, message) = await _otp.VerifyAsync(req.Phone, req.Code);
            if (!ok) return BadRequest(message);

            return Ok(new { message });
        }
    }
}
