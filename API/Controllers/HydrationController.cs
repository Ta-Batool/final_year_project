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
            var items = await _hydrationService.GetForDayAsync(clientId, todayUtc);
            return Ok(items);
        }

        // GET api/hydration/by-date/{clientId}?date=2025-12-01
        [HttpGet("by-date/{clientId}")]
        public async Task<ActionResult<List<HydrationLog>>> GetByDate(string clientId, [FromQuery] DateTime date)
        {
            var dateUtc = date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();
            var items = await _hydrationService.GetForDayAsync(clientId, dateUtc);
            return Ok(items);
        }

        // POST api/hydration
        [HttpPost]
        public async Task<ActionResult<HydrationLog>> Create(HydrationLog log)
        {
            var created = await _hydrationService.AddAsync(log);
            return CreatedAtAction(nameof(GetToday),
                new { clientId = created.ClientId },
                created);
        }

        // DELETE api/hydration/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _hydrationService.DeleteAsync(id);
            return NoContent();
        }
    }
}
