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

        // helper: map DB entry -> DTO
        private static ExerciseLog ToLog(ExerciseEntry e) => new ExerciseLog
        {
            Id = e.Id,
            ClientId = e.ClientId,
            Name = e.Name,
            Type = e.Type,
            DurationMinutes = e.DurationMinutes ?? 0,
            Intensity = e.Intensity,
            Date = e.Date,
            CreatedAt = e.CreatedAt,
            CaloriesBurned = e.CaloriesBurned
        };

        // helper: map DTO -> DB entry
        private static ExerciseEntry ToEntry(ExerciseLog log) => new ExerciseEntry
        {
            Id = log.Id,
            ClientId = log.ClientId!,
            Name = log.Name,
            Type = log.Type,
            DurationMinutes = log.DurationMinutes,
            Intensity = log.Intensity,
            Date = log.Date,
            CreatedAt = log.CreatedAt,
            CaloriesBurned = log.CaloriesBurned,
            Status = ExerciseStatus.Done // use existing enum value
        };

        // GET api/exercises/today/{clientId}
        [HttpGet("today/{clientId}")]
        public async Task<ActionResult<List<ExerciseLog>>> GetToday(string clientId)
        {
            var todayUtc = DateTime.UtcNow.Date;
            var entries = await _exerciseService.GetForDayAsync(clientId, todayUtc);

            var logs = new List<ExerciseLog>();
            foreach (var e in entries)
            {
                logs.Add(ToLog(e));
            }

            return Ok(logs);
        }

        // GET api/exercises/by-date?clientId=...&date=2025-12-01
        [HttpGet("by-date")]
        public async Task<ActionResult<List<ExerciseLog>>> GetByDate(
            [FromQuery] string clientId,
            [FromQuery] DateTime date)
        {
            var dateUtc = date.Kind == DateTimeKind.Utc ? date.Date : date.ToUniversalTime().Date;
            var entries = await _exerciseService.GetForDayAsync(clientId, dateUtc);

            var logs = new List<ExerciseLog>();
            foreach (var e in entries)
            {
                logs.Add(ToLog(e));
            }

            return Ok(logs);
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

            // map DTO -> DB entity
            var entry = ToEntry(log);

            var createdEntry = await _exerciseService.AddAsync(entry);
            var createdLog = ToLog(createdEntry);

            return CreatedAtAction(nameof(GetToday),
                new { clientId = createdLog.ClientId },
                createdLog);
        }

        // DELETE api/exercises/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _exerciseService.DeleteAsync(id);
            return NoContent();
        }

        // 🔍 SEARCH: external calories API via ICaloriesBurnedApiService
        // GET api/exercises/search?query=run&weightKg=70
        [HttpGet("search")]
        public async Task<ActionResult<List<ExerciseSuggestion>>> Search(
            [FromQuery] string query,
            [FromQuery] int? weightKg = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<ExerciseSuggestion>());

            var suggestions = await _caloriesApi.SearchExercisesAsync(query, weightKg);
            return Ok(suggestions);
        }
    }
}
