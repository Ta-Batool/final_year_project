using API.Services;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkoutController : ControllerBase
    {
        private readonly WorkoutService _workouts;

        public WorkoutController(WorkoutService workouts)
        {
            _workouts = workouts;
        }

        [HttpGet("exercises")]
        public async Task<IActionResult> Exercises()
            => Ok(await _workouts.GetExercisesAsync());

        [HttpPost("exercises")]
        public async Task<IActionResult> CreateExercise([FromBody] WorkoutExercise x)
        {
            if (string.IsNullOrWhiteSpace(x.Name)) return BadRequest("Name required");
            return Ok(await _workouts.CreateExerciseAsync(x));
        }

        [HttpDelete("exercises/{id}")]
        public async Task<IActionResult> DeleteExercise(string id)
        {
            await _workouts.DeleteExerciseAsync(id);
            return Ok();
        }

        [HttpGet("routines")]
        public async Task<IActionResult> Routines()
            => Ok(await _workouts.GetRoutinesAsync());

        [HttpPost("routines")]
        public async Task<IActionResult> CreateRoutine([FromBody] WorkoutRoutine r)
        {
            if (string.IsNullOrWhiteSpace(r.Name)) return BadRequest("Name required");
            if (r.Items.Count == 0) return BadRequest("Routine needs items");

            return Ok(await _workouts.CreateRoutineAsync(r));
        }

        [HttpDelete("routines/{id}")]
        public async Task<IActionResult> DeleteRoutine(string id)
        {
            await _workouts.DeleteRoutineAsync(id);
            return Ok();
        }
    }
}
