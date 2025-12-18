using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/healthlogs/glucose")]
    public class GlucoseLogsController : ControllerBase
    {
        private readonly GlucoseLogService _service;

        public GlucoseLogsController(GlucoseLogService service)
        {
            _service = service;
        }

        // GET api/healthlogs/glucose/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var logs = await _service.GetByUserAsync(userId);
            return Ok(logs);
        }

        // POST api/healthlogs/glucose
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GlucoseLog log)
        {
            if (log == null || string.IsNullOrWhiteSpace(log.UserId))
                return BadRequest("Invalid glucose log");

            log.LoggedAt = DateTime.UtcNow;

            var created = await _service.CreateAsync(log);
            return Ok(created);
        }

        // DELETE api/healthlogs/glucose/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
