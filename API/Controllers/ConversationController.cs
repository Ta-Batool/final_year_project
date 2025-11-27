using API.Services;
using Microsoft.AspNetCore.Mvc;
using Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;

        public ConversationController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        // 🔹 All conversations for a given client (doctor or patient)
        // GET api/conversation/for/{clientId}
        [HttpGet("for/{clientId}")]
        public async Task<ActionResult<IEnumerable<Conversation>>> GetForClient(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return BadRequest("clientId is required.");

            var list = await _conversationService.GetAllForParticipantAsync(clientId);
            return Ok(list);
        }

        // 🔹 Single conversation by id
        [HttpGet("{id}")]
        public async Task<ActionResult<Conversation>> GetById(string id)
        {
            var conv = await _conversationService.GetByIdAsync(id);
            if (conv == null) return NotFound();
            return Ok(conv);
        }

        // 🔹 Create a new group conversation
        [HttpPost]
        public async Task<ActionResult<Conversation>> Create([FromBody] Conversation conversation)
        {
            if (conversation == null || conversation.ParticipantIds == null || conversation.ParticipantIds.Count == 0)
                return BadRequest("At least one participant is required.");

            conversation.IsGroup = true;
            conversation.CreatedAt = DateTime.UtcNow;

            var created = await _conversationService.CreateAsync(conversation);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // 🔹 Add a participant to group
        // POST api/conversation/{id}/add-participant?clientId=...
        [HttpPost("{id}/add-participant")]
        public async Task<IActionResult> AddParticipant(string id, [FromQuery] string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return BadRequest("clientId is required.");

            await _conversationService.AddParticipantAsync(id, clientId);
            return NoContent();
        }
    }
}
