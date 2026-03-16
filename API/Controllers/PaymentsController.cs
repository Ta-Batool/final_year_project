using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class SubscribeRequest
    {
        public string ClientId { get; set; } = "";
        public string CardNumber { get; set; } = "";
        public string CardName { get; set; } = "";
        public string Expiry { get; set; } = "";
        public string Cvv { get; set; } = "";
        public int AmountPkr { get; set; } = 1500;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _payments;

        public PaymentsController(PaymentService payments)
        {
            _payments = payments;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.ClientId))
                return BadRequest("ClientId required");

            if (string.IsNullOrWhiteSpace(req.CardNumber))
                return BadRequest("Card number required");

            // ✅ This must:
            // 1) create payment record
            // 2) set client.IsPremium = true in MongoDB
            var rec = await _payments.SubscribeAsync(req.ClientId, req.CardNumber, req.AmountPkr);

            return Ok(rec);
        }

        [HttpGet]
        public async Task<IActionResult> All()
            => Ok(await _payments.GetAllAsync());

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> ByClient(string clientId)
            => Ok(await _payments.GetByClientAsync(clientId));
    }
}
