using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class CoachChatRequest
    {
        public string ClientId { get; set; } = "";
        public string Message { get; set; } = "";
        public int Year { get; set; }
        public int Month { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class FitnessCoachController : ControllerBase
    {
        private readonly FitnessCoachService _coach;

        public FitnessCoachController(FitnessCoachService coach)
        {
            _coach = coach;
        }

        [HttpGet("summary/{clientId}/{year:int}/{month:int}")]
        public async Task<IActionResult> Summary(string clientId, int year, int month)
        {
            var data = await _coach.GetMonthlySummaryAsync(clientId, year, month);
            return Ok(data);
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] CoachChatRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.ClientId)) return BadRequest("ClientId required");
            if (string.IsNullOrWhiteSpace(req.Message)) return BadRequest("Message required");

            var data = await _coach.ChatAsync(req.ClientId, req.Message, req.Year, req.Month);
            return Ok(data);
        }
    }
}
