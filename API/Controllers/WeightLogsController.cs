using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/healthlogs/weight")]
    public class WeightLogsController : ControllerBase
    {
        private readonly WeightLogService _service;

        public WeightLogsController(WeightLogService service)
        {
            _service = service;
        }

        // GET api/healthlogs/weight/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var logs = await _service.GetByUserAsync(userId);
            return Ok(logs);
        }

        // POST api/healthlogs/weight
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WeightLog log)
        {
            if (log == null || string.IsNullOrWhiteSpace(log.UserId))
                return BadRequest("Invalid weight log");

            log.LoggedAt = DateTime.UtcNow;

            var created = await _service.CreateAsync(log);
            return Ok(created);
        }

        // DELETE api/healthlogs/weight/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
