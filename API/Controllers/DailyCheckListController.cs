using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DailyChecklistController : ControllerBase
    {
        private readonly DailyChecklistService _svc;
        public DailyChecklistController(DailyChecklistService svc) => _svc = svc;

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(string userId, [FromQuery] DateTime date)
            => Ok(await _svc.GetByUserAndDateAsync(userId, date));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DailyChecklist c)
        {
            c.Date = c.Date.Date;
            await _svc.CreateAsync(c);
            return Ok(c);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] DailyChecklist c)
        {
            c.Id = id;
            c.Date = c.Date.Date;
            await _svc.UpdateAsync(id, c);
            return NoContent();
        }
    }
}
