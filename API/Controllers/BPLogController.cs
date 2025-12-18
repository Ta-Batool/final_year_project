using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/healthlogs/bp")]
    public class BPLogsController : ControllerBase
    {
        private readonly BPLogService _service;

        public BPLogsController(BPLogService service)
        {
            _service = service;
        }

        // GET api/healthlogs/bp/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var logs = await _service.GetByUserAsync(userId);
            return Ok(logs);
        }

        // POST api/healthlogs/bp
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BPLog log)
        {
            if (log == null || string.IsNullOrWhiteSpace(log.UserId))
                return BadRequest("Invalid BP log");

            log.LoggedAt = DateTime.UtcNow;

            var created = await _service.CreateAsync(log);
            return Ok(created);
        }

        // DELETE api/healthlogs/bp/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
