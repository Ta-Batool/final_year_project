using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Model;
using Stripe.Checkout;

namespace API.Services
{
    public class PaymentService
    {
        private readonly IMongoCollection<PaymentRecord> _payments;
        private readonly IMongoCollection<Client> _clients;
        private readonly IConfiguration _config;

        public string BlazorBaseUrl { get; }

        public PaymentService(IMongoDatabase database, IConfiguration config)
        {
            _payments = database.GetCollection<PaymentRecord>("Payments");
            _clients = database.GetCollection<Client>("Client");
            _config = config;

            BlazorBaseUrl = _config["BLAZOR_BASE_URL"] ?? "https://localhost:7126";
        }

        public async Task<string> CreateStripeCheckoutSessionAsync(string clientId, int amountPkr)
        {
            if (amountPkr <= 0)
                amountPkr = 1500;

            var apiBaseUrl = _config["API_BASE_URL"] ?? "https://localhost:7191";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                SuccessUrl = $"{apiBaseUrl}/api/Payments/success?sessionId={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{apiBaseUrl}/api/Payments/cancel",
                ClientReferenceId = clientId,

                Metadata = new Dictionary<string, string>
                {
                    { "clientId", clientId },
                    { "amountPkr", amountPkr.ToString() }
                },

                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "pkr",
                            UnitAmount = amountPkr * 100,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "NutriNest Premium Subscription",
                                Description = "Premium access for personalized nutrition and fitness features"
                            }
                        }
                    }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            var payment = new PaymentRecord
            {
                ClientId = clientId,
                AmountPkr = amountPkr,
                StripeSessionId = session.Id,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };

            await _payments.InsertOneAsync(payment);

            return session.Url;
        }

        public async Task ConfirmStripePaymentAsync(string sessionId)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            if (session.PaymentStatus != "paid")
                throw new Exception("Payment not completed.");

            var clientId = session.ClientReferenceId;

            if (string.IsNullOrWhiteSpace(clientId) && session.Metadata.ContainsKey("clientId"))
                clientId = session.Metadata["clientId"];

            if (string.IsNullOrWhiteSpace(clientId))
                throw new Exception("ClientId missing from Stripe session.");

            await _payments.UpdateOneAsync(
                p => p.StripeSessionId == sessionId,
                Builders<PaymentRecord>.Update
                    .Set(p => p.Status, "PAID")
                    .Set(p => p.PaidAt, DateTime.UtcNow)
            );

            var result = await _clients.UpdateOneAsync(
                c => c.Id == clientId,
                Builders<Client>.Update.Set(c => c.IsPremium, true)
            );

            if (result.MatchedCount == 0)
                throw new Exception("Client not found. Premium not updated.");
        }

        public async Task<List<PaymentRecord>> GetAllAsync()
            => await _payments.Find(_ => true).ToListAsync();

        public async Task<List<PaymentRecord>> GetByClientAsync(string clientId)
            => await _payments.Find(p => p.ClientId == clientId).ToListAsync();
    }

    public class PaymentRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string ClientId { get; set; } = "";
        public int AmountPkr { get; set; }

        public string StripeSessionId { get; set; } = "";
        public string Status { get; set; } = "PENDING";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}