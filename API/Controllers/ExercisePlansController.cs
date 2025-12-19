using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExercisePlansController : ControllerBase
    {
        private readonly ExercisePlanService _service;

        public ExercisePlansController(ExercisePlanService service)
        {
            _service = service;
        }

        // POST: /api/exerciseplans/build
        // Body: MetabolismSummary (sent from frontend)
        [HttpPost("build")]
        public async Task<ActionResult<ExercisePlanResult>> Build([FromBody] MetabolismSummary meta)
        {
            if (meta == null)
                return BadRequest("MetabolismSummary is required.");

            var plan = await _service.BuildAsync(meta);
            return Ok(plan);
        }
    }
}
