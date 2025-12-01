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
        private readonly ICaloriesBurnedApiService _caloriesApi;

        public ExercisesController(
            IExerciseService exerciseService,
            ICaloriesBurnedApiService caloriesApi)
        {
            _exerciseService = exerciseService;
            _caloriesApi = caloriesApi;
        }

        // GET api/exercises/today/{clientId}
        [HttpGet("today/{clientId}")]
        public async Task<ActionResult<List<ExerciseLog>>> GetToday(string clientId)
        {
            var todayUtc = DateTime.UtcNow.Date;
            var items = await _exerciseService.GetForDayAsync(clientId, todayUtc);
            return Ok(items);
        }

        // GET api/exercises/by-date?clientId=...&date=2025-12-01
        [HttpGet("by-date")]
        public async Task<ActionResult<List<ExerciseLog>>> GetByDate(
            [FromQuery] string clientId,
            [FromQuery] DateTime date)
        {
            var dateUtc = date.Kind == DateTimeKind.Utc ? date.Date : date.ToUniversalTime().Date;
            var items = await _exerciseService.GetForDayAsync(clientId, dateUtc);
            return Ok(items);
        }

        // GET api/exercises/client/{clientId}
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<List<ExerciseLog>>> GetAllForClient(string clientId)
        {
            var items = await _exerciseService.GetAllForClientAsync(clientId);
            return Ok(items);
        }

        // POST api/exercises
        [HttpPost]
        public async Task<ActionResult<ExerciseLog>> Create([FromBody] ExerciseLog log)
        {
            if (log == null)
                return BadRequest("Exercise log is required.");

            if (string.IsNullOrWhiteSpace(log.ClientId))
                return BadRequest("ClientId is required.");

            var nowUtc = DateTime.UtcNow;

            if (log.Date == default)
                log.Date = nowUtc.Date;

            log.CreatedAt = nowUtc;
            log.Id = null; // let Mongo assign

            var created = await _exerciseService.CreateAsync(log);

            return CreatedAtAction(nameof(GetToday),
                new { clientId = created.ClientId },
                created);
        }

        // DELETE api/exercises/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _exerciseService.DeleteAsync(id);
            return NoContent();
        }

        // 🔍 NOW USING EXTERNAL API (API Ninjas via ICaloriesBurnedApiService)
        // GET api/exercises/search?query=run
        [HttpGet("search")]
        public async Task<ActionResult<List<ExerciseSuggestion>>> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<ExerciseSuggestion>());

            var suggestions = await _caloriesApi.SearchExercisesAsync(query);
            return Ok(suggestions);
        }
    }
}
