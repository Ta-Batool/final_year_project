using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class CreateCheckoutRequest
    {
        public string ClientId { get; set; } = "";
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

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.ClientId))
                return BadRequest("ClientId required");

            var url = await _payments.CreateStripeCheckoutSessionAsync(req.ClientId, req.AmountPkr);
            return Ok(new { checkoutUrl = url });
        }

        [HttpGet("success")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return BadRequest("Session id missing");

            await _payments.ConfirmStripePaymentAsync(sessionId);
            return Redirect($"{_payments.BlazorBaseUrl}/userdashboard?payment=success");
        }

        [HttpGet("cancel")]
        public IActionResult PaymentCancel()
        {
            return Redirect($"{_payments.BlazorBaseUrl}/patient/subscribe?payment=cancel");
        }

        [HttpGet]
        public async Task<IActionResult> All()
            => Ok(await _payments.GetAllAsync());

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> ByClient(string clientId)
            => Ok(await _payments.GetByClientAsync(clientId));
    }
}