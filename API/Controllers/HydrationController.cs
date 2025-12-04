using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HydrationController : ControllerBase
    {
        private readonly IHydrationService _hydrationService;

        public HydrationController(IHydrationService hydrationService)
        {
            _hydrationService = hydrationService;
        }

        // GET api/hydration/today/{clientId}
        [HttpGet("today/{clientId}")]
        public async Task<ActionResult<List<HydrationLog>>> GetToday(string clientId)
        {
            var todayUtc = DateTime.UtcNow.Date;
            var logs = await _hydrationService.GetForDayAsync(clientId, todayUtc);
            return Ok(logs);
        }

        // GET api/hydration/by-date/{clientId}?date=
        [HttpGet("by-date/{clientId}")]
        public async Task<ActionResult<List<HydrationLog>>> GetByDate(string clientId, [FromQuery] DateTime date)
        {
            var dateUtc = date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();
            var logs = await _hydrationService.GetForDayAsync(clientId, dateUtc);
            return Ok(logs);
        }

        // POST api/hydration  (create raw hydration entry)
        [HttpPost]
        public async Task<ActionResult<HydrationLog>> Create([FromBody] HydrationLog log)
        {
            var created = await _hydrationService.AddAsync(log);
            return CreatedAtAction(nameof(GetToday),
                new { clientId = created.ClientId },
                created);
        }

        // ❗ NEW ENDPOINT: ADD WATER
        // POST api/hydration/add
        [HttpPost("add")]
        public async Task<IActionResult> AddWater([FromBody] HydrationAddRequest req)
        {
            if (req == null) return BadRequest("Invalid request");

            await _hydrationService.AddWaterAsync(req.ClientId, req.AmountMl);
            return Ok();
        }

        // ❗ NEW ENDPOINT: UPDATE DAILY TARGET
        // POST api/hydration/target
        [HttpPost("target")]
        public async Task<IActionResult> UpdateTarget([FromBody] HydrationTargetRequest req)
        {
            if (req == null) return BadRequest("Invalid request");

            await _hydrationService.UpdateTargetAsync(req.ClientId, req.TargetMl);
            return Ok();
        }

        // DELETE api/hydration/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _hydrationService.DeleteAsync(id);
            return NoContent();
        }
    }

    // DTOs for Blazor API calls
    public class HydrationAddRequest
    {
        public string ClientId { get; set; } = null!;
        public int AmountMl { get; set; }
    }

    public class HydrationTargetRequest
    {
        public string ClientId { get; set; } = null!;
        public int TargetMl { get; set; }
    }
}
