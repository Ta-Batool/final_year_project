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

        [HttpGet]
        public async Task<ActionResult<List<Client>>> GetAll()
        {
            return await _clientService.GetAllAsync();
        }

        [HttpGet("{email}")]
        public async Task<ActionResult<Client>> GetByEmail(string email)
        {
            var client = await _clientService.GetByEmailAsync(email);
            if (client == null) return NotFound();
            return client;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Client client)
        {
            await _clientService.CreateAsync(client);
            return CreatedAtAction(nameof(GetByEmail), new { email = client.Email }, client);
        }

        [HttpPut("{email}")]
        public async Task<IActionResult> Update(string email, Client client)
        {
            await _clientService.UpdateAsync(email, client);
            return NoContent();
        }

        [HttpDelete("{email}")]
        public async Task<IActionResult> Delete(string email)
        {
            await _clientService.DeleteAsync(email);
            return NoContent();
        }
    }
}
