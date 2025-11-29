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
    public class MealsController : ControllerBase
    {
        private readonly IMealService _mealService;

        public MealsController(IMealService mealService)
        {
            _mealService = mealService;
        }

        // GET api/meals/today/{clientId}
        [HttpGet("today/{clientId}")]
        public async Task<ActionResult<List<Meal>>> GetToday(string clientId)
        {
            var todayUtc = DateTime.UtcNow.Date;
            var meals = await _mealService.GetMealsForDayAsync(clientId, todayUtc);
            return Ok(meals);
        }

        // ✅ NEW: GET api/meals/by-date?clientId=...&date=2025-11-29
        [HttpGet("by-date")]
        public async Task<ActionResult<List<Meal>>> GetByDate(
            [FromQuery] string clientId,
            [FromQuery] DateTime date)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return BadRequest("clientId is required.");

            var meals = await _mealService.GetMealsByDateAsync(clientId, date);
            return Ok(meals);
        }

        // GET api/meals/by-client/{clientId}
        [HttpGet("by-client/{clientId}")]
        public async Task<ActionResult<List<Meal>>> GetByClient(string clientId)
        {
            var meals = await _mealService.GetAllForClientAsync(clientId);
            return Ok(meals);
        }

        // POST api/meals
        [HttpPost]
        public async Task<ActionResult<Meal>> Create([FromBody] Meal mealDto)
        {
            if (mealDto == null)
                return BadRequest("Meal is required.");

            if (string.IsNullOrWhiteSpace(mealDto.ClientId))
                return BadRequest("ClientId is required.");

            var nowUtc = DateTime.UtcNow;

            if (mealDto.Date == default)
                mealDto.Date = nowUtc.Date;

            mealDto.CreatedAt = nowUtc;
            mealDto.Id = null; // let Mongo create new Id

            var created = await _mealService.CreateAsync(mealDto);

            return CreatedAtAction(nameof(GetToday),
                new { clientId = created.ClientId },
                created);
        }

        // DELETE api/meals/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _mealService.DeleteAsync(id);
            return NoContent();
        }
    }
}
