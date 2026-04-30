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
        private readonly CheckInService _checkInService;

        public MealsController(IMealService mealService, CheckInService checkInService)
        {
            _mealService = mealService;
            _checkInService = checkInService;
        }

        // GET api/meals/today/{clientId}
        [HttpGet("today/{clientId}")]
        public async Task<ActionResult<List<Meal>>> GetToday(string clientId)
        {
            var todayUtc = DateTime.UtcNow.Date;
            var meals = await _mealService.GetMealsForDayAsync(clientId, todayUtc);
            return Ok(meals);
        }

        // GET api/meals/by-date?clientId=...&date=2026-03-02
        [HttpGet("by-date")]
        public async Task<ActionResult<List<Meal>>> GetByDate(
            [FromQuery] string clientId,
            [FromQuery] DateTime date)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return BadRequest("clientId is required.");

            var meals = await _mealService.GetMealsByDateAsync(clientId, date.Date);
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
            else
                mealDto.Date = mealDto.Date.Date;

            mealDto.CreatedAt = nowUtc;
            mealDto.Id = null;

            var created = await _mealService.CreateAsync(mealDto);

            // After meal add, ensure a DailyCheckIn exists for that same date
            var sameDayMeals = await _mealService.GetMealsForDayAsync(created.ClientId, created.Date.Date);
            var mealSummary = BuildFoodNotes(sameDayMeals);

            await _checkInService.UpsertAsync(new DailyCheckIn
            {
                ClientId = created.ClientId,
                DateUtc = created.Date.Date,
                FoodNotes = mealSummary
            });

            return CreatedAtAction(nameof(GetToday), new { clientId = created.ClientId }, created);
        }

        // DELETE api/meals/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _mealService.DeleteAsync(id);
            return NoContent();
        }

        private static string BuildFoodNotes(List<Meal> meals)
        {
            if (meals == null || meals.Count == 0)
                return "Meal logged";

            var parts = meals
                .OrderBy(m => m.CreatedAt)
                .Select(m =>
                {
                    var foods = string.IsNullOrWhiteSpace(m.Foods) ? "Meal" : m.Foods.Trim();
                    var type = string.IsNullOrWhiteSpace(m.Type) ? "Meal" : m.Type.Trim();
                    var calories = m.Calories.HasValue ? $" ({m.Calories.Value} kcal)" : "";
                    return $"{type}: {foods}{calories}";
                });

            return string.Join(" | ", parts);
        }
    }
}