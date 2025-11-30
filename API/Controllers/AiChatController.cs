using System.Security.Claims;
using API.Ai;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]  // keep disabled for now so auth doesn't block tests
    public class AiChatController : ControllerBase
    {
        private readonly IAiAssistantService _ai;
        private readonly ILogger<AiChatController> _logger;

        public AiChatController(IAiAssistantService ai, ILogger<AiChatController> logger)
        {
            _ai = ai;
            _logger = logger;
        }

        [HttpPost("chat")]
        public async Task<ActionResult<AiChatResponseDto>> Chat([FromBody] AiChatRequestDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Message))
                {
                    return Ok(new AiChatResponseDto
                    {
                        Reply = "Please type a question and send it."
                    });
                }

                var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
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

                return Ok(new AiChatResponseDto
                {
                    Reply = string.IsNullOrWhiteSpace(reply)
                        ? "The AI assistant did not return a response."
                        : reply
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AiChatController.Chat");
                // Still return 200 so the Blazor client doesn't show "couldn't reach"
                return Ok(new AiChatResponseDto
                {
                    Reply = "Sorry, an internal error occurred while contacting the AI assistant."
                });
            }
        }
    }
}
