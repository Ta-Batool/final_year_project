using API.Services;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        // ✅ Admin list (also used by AdminUsers page)
        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<List<Client>>> GetAll()
        {
            var list = await _clientService.GetAllAsync();
            return Ok(list);
        }

        // ✅ Use for patient login -> resolve client record
        // NOTE: email can contain '.' etc so keep it as route param but ensure FE uses Uri.EscapeDataString
        // GET: api/Clients/{email}
        [HttpGet("{email}")]
        public async Task<ActionResult<Client>> GetByEmail(string email)
        {
            var client = await _clientService.GetByEmailAsync(email);
            if (client == null) return NotFound();
            return Ok(client);
        }

        // ✅ Create client (signup)
        // POST: api/Clients
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Client client)
        {
            if (client == null) return BadRequest("Client payload required.");
            if (string.IsNullOrWhiteSpace(client.Email)) return BadRequest("Email is required.");

            // Prevent duplicate
            var existing = await _clientService.GetByEmailAsync(client.Email);
            if (existing != null)
                return Conflict("Client already exists.");

            await _clientService.CreateAsync(client);
            return CreatedAtAction(nameof(GetByEmail), new { email = client.Email }, client);
        }

        // ✅ Update by email
        // PUT: api/Clients/{email}
        [HttpPut("{email}")]
        public async Task<IActionResult> Update(string email, [FromBody] Client client)
        {
            if (client == null) return BadRequest("Client payload required.");
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email is required.");

            var existing = await _clientService.GetByEmailAsync(email);
            if (existing == null) return NotFound();

            // ensure email identity doesn't drift
            client.Email = existing.Email;
            client.Id = existing.Id;

            await _clientService.UpdateAsync(email, client);
            return NoContent();
        }

        // ✅ Delete by email
        // DELETE: api/Clients/{email}
        [HttpDelete("{email}")]
        public async Task<IActionResult> Delete(string email)
        {
            var existing = await _clientService.GetByEmailAsync(email);
            if (existing == null) return NotFound();

            await _clientService.DeleteAsync(email);
            return NoContent();
        }

        // ==========================================================
        // ✅ NEW: Premium / Paid System helpers
        // ==========================================================

        // ✅ Mark client premium/unpremium (admin or dummy payment callback)
        // PATCH: api/Clients/{email}/premium?isPremium=true
        [HttpPatch("{email}/premium")]
        public async Task<IActionResult> SetPremium(string email, [FromQuery] bool isPremium = true)
        {
            var existing = await _clientService.GetByEmailAsync(email);
            if (existing == null) return NotFound();

            existing.IsPremium = isPremium;
            await _clientService.UpdateAsync(email, existing);

            return Ok(new { existing.Email, existing.IsPremium });
        }

        // ✅ Get client premium status quick check
        // GET: api/Clients/{email}/premium
        [HttpGet("{email}/premium")]
        public async Task<IActionResult> GetPremium(string email)
        {
            var existing = await _clientService.GetByEmailAsync(email);
            if (existing == null) return NotFound();

            return Ok(new { existing.Email, existing.IsPremium });
        }
    }
}
