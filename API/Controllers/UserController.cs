using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;   // use interface

        public UsersController(IUserService userService)  // inject interface
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User data is required.");
            }

            await _userService.CreateAsync(user);
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, User user)
        {
            var existingUser = await _userService.GetByIdAsync(id);
            if (existingUser == null)
                return NotFound();

            user.Id = id;
            await _userService.UpdateAsync(id, user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var existingUser = await _userService.GetByIdAsync(id);
            if (existingUser == null)
                return NotFound();

            await _userService.DeleteAsync(id);
            return NoContent();
        }
        [HttpPut("by-client/{clientId}")]
        public async Task<IActionResult> UpdateUserByClientId(string clientId, [FromBody] User updatedUser)
        {
            if (updatedUser == null)
                return BadRequest("updatedUser cannot be null");

            var existingUser = await _userService.GetUserByClientIdAsync(clientId);
            if (existingUser == null)
                return NotFound();

            updatedUser.Id = existingUser.Id; // keep same ID for MongoDB
            await _userService.UpdateUserAsync(existingUser.Id, updatedUser);
            return NoContent();
        }
        [HttpGet("by-client/{clientId}")]
        public async Task<ActionResult<User>> GetByClientId(string clientId)
        {
            var user = await _userService.GetUserByClientIdAsync(clientId);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
    }
}
