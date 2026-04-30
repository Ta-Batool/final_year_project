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

        public record SendOtpRequest(string CountryIso, string Phone);
        public record VerifyOtpRequest(string CountryIso, string Phone, string Code);

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendOtpRequest req)
        {
            var result = await _otp.SendAsync(req.CountryIso, req.Phone);

            if (!result.ok)
                return BadRequest(new { message = result.message });

            return Ok(new { message = result.message });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyOtpRequest req)
        {
            var result = await _otp.VerifyAsync(req.CountryIso, req.Phone, req.Code);

            if (!result.ok)
                return BadRequest(new { message = result.message });

            return Ok(new
            {
                message = result.message,
                phone = result.e164Phone
            });
        }
    }
}