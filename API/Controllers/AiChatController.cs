using System.Security.Claims;
using API.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // you can comment this while testing if auth causes issues
    public class AiChatController : ControllerBase
    {
        private readonly IAiAssistantService _ai;

        public AiChatController(IAiAssistantService ai)
        {
            _ai = ai;
        }

        [HttpPost("chat")]
        public async Task<ActionResult<AiChatResponseDto>> Chat([FromBody] AiChatRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Message is required.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var role = dto.Role?.Trim().ToLowerInvariant() ?? "patient";

            string reply;
            if (role == "doctor")
            {
                reply = await _ai.GetDoctorReplyAsync(userId, dto.Message);
            }
            else
            {
                reply = await _ai.GetPatientReplyAsync(userId, dto.Message);
            }

            return Ok(new AiChatResponseDto { Reply = reply });
        }
    }
}
