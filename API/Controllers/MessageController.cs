using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly DoctorService _doctorService;
        private readonly IUserService _userService;

        public MessageController(
            IMessageService messageService,
            DoctorService doctorService,
            IUserService userService)
        {
            _messageService = messageService;
            _doctorService = doctorService;
            _userService = userService;
        }

        // 🔹 Get all messages (mainly for admin/debug)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetAll()
        {
            var messages = await _messageService.GetAllAsync();
            return Ok(messages);
        }

        // 🔹 Get single message by id
        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetById(string id)
        {
            var message = await _messageService.GetByIdAsync(id);
            if (message == null) return NotFound();
            return Ok(message);
        }

        // 🔹 Get conversation between one user and one doctor
        //     GET api/message/conversation?userClientId=...&doctorClientId=...
        [HttpGet("conversation")]
        public async Task<ActionResult<IEnumerable<Message>>> GetConversation(
            [FromQuery] string userClientId,
            [FromQuery] string doctorClientId)
        {
            if (string.IsNullOrWhiteSpace(userClientId) ||
                string.IsNullOrWhiteSpace(doctorClientId))
            {
                return BadRequest("userClientId and doctorClientId are required.");
            }

            var messages = await _messageService.GetConversationAsync(userClientId, doctorClientId);
            return Ok(messages);
        }

        // 🔹 Create new message
        [HttpPost]
        public async Task<ActionResult<Message>> Create([FromBody] Message message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Text))
                return BadRequest("Message cannot be empty.");

            if (message.SentAt == default)
            {
                message.SentAt = DateTime.UtcNow;
            }

            await _messageService.CreateAsync(message);

            // Return 201 with the created object
            return CreatedAtAction(nameof(GetById), new { id = message.Id }, message);
        }

        // 🔹 Update
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Message updatedMessage)
        {
            var existing = await _messageService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _messageService.UpdateAsync(id, updatedMessage);
            return NoContent();
        }

        // 🔹 Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _messageService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _messageService.DeleteAsync(id);
            return NoContent();
        }

        // 🔹 For doctor: list userClientIds this doctor has messaged
        //     GET api/message/doctor/{doctorClientId}/users
        [HttpGet("doctor/{doctorClientId}/users")]
        public async Task<ActionResult<IEnumerable<string>>> GetUsersForDoctor(string doctorClientId)
        {
            if (string.IsNullOrWhiteSpace(doctorClientId))
                return BadRequest("doctorClientId is required.");

            var users = await _messageService.GetDistinctUserIdsForDoctorAsync(doctorClientId);
            return Ok(users);
        }

        // 🔹 For user: list doctorClientIds this user has messaged
        //     GET api/message/user/{userClientId}/doctors
        [HttpGet("user/{userClientId}/doctors")]
        public async Task<ActionResult<IEnumerable<string>>> GetDoctorsForUser(string userClientId)
        {
            if (string.IsNullOrWhiteSpace(userClientId))
                return BadRequest("userClientId is required.");

            var doctors = await _messageService.GetDistinctDoctorIdsForUserAsync(userClientId);
            return Ok(doctors);
        }
    }
}
