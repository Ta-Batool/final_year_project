using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.Services;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthLogsController : ControllerBase
    {
        private readonly BPLogService _bp;
        private readonly GlucoseLogService _glucose;
        private readonly WeightLogService _weight;

        public HealthLogsController(BPLogService bp, GlucoseLogService glucose, WeightLogService weight)
        {
            _bp = bp;
            _glucose = glucose;
            _weight = weight;
        }

        // -------- BP --------
        [HttpGet("bp/{userId}")]
        public async Task<IActionResult> GetBp(string userId) => Ok(await _bp.GetByUserAsync(userId));

        [HttpPost("bp")]
        public async Task<IActionResult> AddBp([FromBody] BPLog log)
        {
            var saved = await _bp.CreateAsync(log);
            var alert = HealthAlertRules.Evaluate(saved);
            return Ok(new { saved, alert });
        }

        // -------- Glucose --------
        [HttpGet("glucose/{userId}")]
        public async Task<IActionResult> GetGlucose(string userId) => Ok(await _glucose.GetByUserAsync(userId));

        [HttpPost("glucose")]
        public async Task<IActionResult> AddGlucose([FromBody] GlucoseLog log)
        {
            var saved = await _glucose.CreateAsync(log);
            var alert = HealthAlertRules.Evaluate(saved);
            return Ok(new { saved, alert });
        }

        // -------- Weight --------
        [HttpGet("weight/{userId}")]
        public async Task<IActionResult> GetWeight(string userId) => Ok(await _weight.GetByUserAsync(userId));

        [HttpPost("weight")]
        public async Task<IActionResult> AddWeight([FromBody] WeightLog log)
        {
            var saved = await _weight.CreateAsync(log);
            var alert = HealthAlertRules.Evaluate(saved);
            return Ok(new { saved, alert });
        }
    }
}
