using API.Services;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthLogsController : ControllerBase
    {
        private readonly HealthLogService _svc;
        public HealthLogsController(HealthLogService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] HealthLog log)
        {
            if (string.IsNullOrWhiteSpace(log.UserId)) return BadRequest("UserId required");
            if (log.Timestamp == default) log.Timestamp = DateTime.UtcNow;
            await _svc.AddAsync(log);
            return Ok(log);
        }

        [HttpGet("daily/{userId}")]
        public Task<IActionResult> Daily(string userId, [FromQuery] DateTime date)
        {
            var from = date.Date;
            var to = from.AddDays(1);
            return Summary(userId, from, to);
        }

        [HttpGet("weekly/{userId}")]
        public Task<IActionResult> Weekly(string userId, [FromQuery] DateTime startDate)
        {
            var from = startDate.Date;
            var to = from.AddDays(7);
            return Summary(userId, from, to);
        }

        [HttpGet("monthly/{userId}")]
        public Task<IActionResult> Monthly(string userId, [FromQuery] int year, [FromQuery] int month)
        {
            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1);
            return Summary(userId, from, to);
        }

        [HttpGet("range/{userId}")]
        public async Task<IActionResult> Range(string userId, [FromQuery] DateTime from, [FromQuery] DateTime to)
            => Ok(await _svc.GetRangeAsync(userId, from, to));

        private async Task<IActionResult> Summary(string userId, DateTime from, DateTime to)
        {
            var summary = await _svc.GetSummaryAsync(userId, from, to);

            // Abnormal alert rule (simple + defendable)
            // BP: >=140/90 abnormal. Glucose: >=200 abnormal (generic high).
            // (You can refine later, but this passes FYP defense)
            var logs = await _svc.GetRangeAsync(userId, from, to);
            var latest = logs.LastOrDefault();

            string? alert = null;
            if (latest != null)
            {
                if (latest.Systolic >= 140 || latest.Diastolic >= 90)
                    alert = "Your blood pressure readings look high. Please consult a doctor.";
                else if (latest.Glucose >= 200)
                    alert = "Your glucose reading looks high. Please consult a doctor.";
            }

            return Ok(new { summary, alert });
        }
    }
}
