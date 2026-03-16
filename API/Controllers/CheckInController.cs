using API.Services;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckInController : ControllerBase
    {
        private readonly CheckInService _service;

        public CheckInController(CheckInService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Upsert([FromBody] DailyCheckIn dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ClientId)) return BadRequest("ClientId required");
            var saved = await _service.UpsertAsync(dto);
            return Ok(saved);
        }

        [HttpGet("{clientId}/month/{year:int}/{month:int}")]
        public async Task<IActionResult> GetMonth(string clientId, int year, int month)
        {
            var list = await _service.GetMonthAsync(clientId, year, month);
            return Ok(list);
        }
    }
}
