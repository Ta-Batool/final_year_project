using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetAll()
        {
            var messages = await _messageService.GetAllAsync();
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetById(string id)
        {
            var message = await _messageService.GetByIdAsync(id);
            if (message == null) return NotFound();
            return Ok(message);
        }

        [HttpGet("conversation")]
        public async Task<ActionResult<IEnumerable<Message>>> GetConversation(
            [FromQuery] string userClientId,
            [FromQuery] string doctorClientId)
        {
            if (string.IsNullOrWhiteSpace(userClientId) || string.IsNullOrWhiteSpace(doctorClientId))
                return BadRequest("userClientId and doctorClientId are required.");

            var messages = await _messageService.GetConversationAsync(userClientId, doctorClientId);
            return Ok(messages);
        }

        [HttpGet("by-conversation/{conversationId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetByConversationId(string conversationId)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
                return BadRequest("conversationId is required.");

            var messages = await _messageService.GetByConversationIdAsync(conversationId);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<ActionResult<Message>> Create([FromBody] Message message)
        {
            if (message == null || (string.IsNullOrWhiteSpace(message.Text) && message.AttachmentData == null))
                return BadRequest("Message cannot be completely empty.");

            if (message.SentAt == default)
                message.SentAt = DateTime.UtcNow;

            await _messageService.CreateAsync(message);
            return CreatedAtAction(nameof(GetById), new { id = message.Id }, message);
        }

        // ✅ Upload endpoint (Swagger-safe + larger request support)
        [HttpPost("with-attachment")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [RequestSizeLimit(25_000_000)] // 25 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 25_000_000)]
        [ApiExplorerSettings(IgnoreApi = true)] // ✅ OPTION A: prevent Swagger crash
        public async Task<ActionResult<Message>> CreateWithAttachment(
            [FromForm] IFormFile file,
            [FromForm] string senderId,
            [FromForm] string senderName,
            [FromForm] string receiverId,
            [FromForm] string receiverName,
            [FromForm] string userClientId,
            [FromForm] string doctorClientId,
            [FromForm] string? text,
            [FromForm] bool isVoice = false,
            [FromForm] string? conversationId = null)
        {
            if (file == null || file.Length <= 0)
                return BadRequest("File is required.");

            if (string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(receiverId))
                return BadRequest("senderId and receiverId are required.");

            byte[] data;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                data = ms.ToArray();
            }

            var message = new Message
            {
                SenderId = senderId,
                SenderName = senderName ?? "",
                ReceiverId = receiverId,
                ReceiverName = receiverName ?? "",
                UserClientId = userClientId ?? "",
                DoctorClientId = doctorClientId ?? "",
                Text = text ?? string.Empty,
                SentAt = DateTime.UtcNow,
                AttachmentFileName = file.FileName,
                AttachmentContentType = file.ContentType,
                AttachmentData = data,
                IsVoiceMessage = isVoice,
                ConversationId = conversationId
            };

            await _messageService.CreateAsync(message);

            return CreatedAtAction(nameof(GetById), new { id = message.Id }, message);
        }

        [HttpGet("{id}/attachment")]
        public async Task<IActionResult> GetAttachment(string id)
        {
            var message = await _messageService.GetByIdAsync(id);
            if (message == null || message.AttachmentData == null)
                return NotFound();

            var contentType = message.AttachmentContentType ?? "application/octet-stream";
            var fileName = message.AttachmentFileName ?? "attachment";

            return File(message.AttachmentData, contentType, fileName);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Message updatedMessage)
        {
            var existing = await _messageService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            updatedMessage.Id = id;
            await _messageService.UpdateAsync(id, updatedMessage);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _messageService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _messageService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("doctor/{doctorClientId}/users")]
        public async Task<ActionResult<IEnumerable<string>>> GetUsersForDoctor(string doctorClientId)
        {
            if (string.IsNullOrWhiteSpace(doctorClientId))
                return BadRequest("doctorClientId is required.");

            var users = await _messageService.GetDistinctUserIdsForDoctorAsync(doctorClientId);
            return Ok(users);
        }

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
