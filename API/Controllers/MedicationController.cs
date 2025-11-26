using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicationsController : ControllerBase
    {
        private readonly IMedicationService _medService;

        public MedicationsController(IMedicationService medService)
        {
            _medService = medService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<MedicationPlan>>> GetPlans(string userId)
        {
            return Ok(await _medService.GetPlansAsync(userId));
        }

        [HttpPost]
        public async Task<IActionResult> Create(MedicationPlan plan)
        {
            await _medService.AddPlanAsync(plan);
            return Ok(plan);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _medService.DeletePlanAsync(id);
            return NoContent();
        }

        [HttpGet("logs/today/{userId}")]
        public async Task<ActionResult<List<MedicationLog>>> Today(string userId)
        {
            return Ok(await _medService.GetTodayLogsAsync(userId));
        }

        [HttpPost("logs/{logId}/status/{status}")]
        public async Task<IActionResult> UpdateStatus(string logId, MedicationStatus status)
        {
            await _medService.UpdateLogStatusAsync(logId, status);
            return NoContent();
        }
    }
}
