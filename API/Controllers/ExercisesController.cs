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
    public class ExercisesController : ControllerBase
    {
        private readonly IExerciseService _exerciseService;

        public ExercisesController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }

        // GET api/exercises/today/{clientId}
        [HttpGet("today/{clientId}")]
        public async Task<ActionResult<List<ExerciseEntry>>> GetToday(string clientId)
        {
            var todayUtc = DateTime.UtcNow.Date;
            var items = await _exerciseService.GetForDayAsync(clientId, todayUtc);
            return Ok(items);
        }

        // GET api/exercises/by-date/{clientId}?date=2025-12-01
        [HttpGet("by-date/{clientId}")]
        public async Task<ActionResult<List<ExerciseEntry>>> GetByDate(string clientId, [FromQuery] DateTime date)
        {
            var dateUtc = date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();
            var items = await _exerciseService.GetForDayAsync(clientId, dateUtc);
            return Ok(items);
        }

        // POST api/exercises
        [HttpPost]
        public async Task<ActionResult<ExerciseEntry>> Create(ExerciseEntry entry)
        {
            var created = await _exerciseService.AddAsync(entry);
            return CreatedAtAction(nameof(GetToday),
                new { clientId = created.ClientId },
                created);
        }

        // PATCH api/exercises/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] ExerciseStatus status)
        {
            await _exerciseService.UpdateStatusAsync(id, status);
            return NoContent();
        }

        // DELETE api/exercises/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _exerciseService.DeleteAsync(id);
            return NoContent();
        }
    }
}
