using MongoDB.Driver;
using Model;

namespace API.Services
{
    public class PaymentService
    {
        private readonly IMongoCollection<PaymentRecord> _payments;
        private readonly IMongoCollection<Client> _clients;

        public PaymentService(IMongoDatabase database)
        {
            _payments = database.GetCollection<PaymentRecord>("Payments");
            _clients = database.GetCollection<Client>("Client");
        }

        public async Task<PaymentRecord> SubscribeAsync(string clientId, string cardNumber, int amountPkr)
        {
            // 1️⃣ Save payment record
            var payment = new PaymentRecord
            {
                ClientId = clientId,
                AmountPkr = amountPkr,
                CardLast4 = cardNumber.Length >= 4 ? cardNumber[^4..] : cardNumber,
                Status = "PAID",
                PaidAt = DateTime.UtcNow
            };

            await _payments.InsertOneAsync(payment);

            // 2️⃣ Update client premium flag
            var result = await _clients.UpdateOneAsync(
                c => c.Id == clientId,
                Builders<Client>.Update.Set(c => c.IsPremium, true)
            );

            if (result.ModifiedCount == 0)
                throw new Exception("Client not found or premium not updated.");

            return payment;
        }

        public async Task<List<PaymentRecord>> GetAllAsync()
            => await _payments.Find(_ => true).ToListAsync();

        public async Task<List<PaymentRecord>> GetByClientAsync(string clientId)
            => await _payments.Find(p => p.ClientId == clientId).ToListAsync();
    }

    public class PaymentRecord
    {
        public string Id { get; set; } = "";
        public string ClientId { get; set; } = "";
        public int AmountPkr { get; set; }
        public string CardLast4 { get; set; } = "";
        public string Status { get; set; } = "PAID";
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    }
}
