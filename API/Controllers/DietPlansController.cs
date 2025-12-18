using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DietPlansController : ControllerBase
    {
        private readonly DietPlanService _svc;
        public DietPlansController(DietPlanService svc) => _svc = svc;

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(string userId, [FromQuery] DateTime date)
            => Ok(await _svc.GetByUserAndDateAsync(userId, date));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DietPlan plan)
        {
            plan.Date = plan.Date.Date;
            await _svc.CreateAsync(plan);
            return Ok(plan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] DietPlan plan)
        {
            plan.Id = id;
            plan.Date = plan.Date.Date;
            await _svc.UpdateAsync(id, plan);
            return NoContent();
        }
    }
}
