using System.Security.Cryptography;
using System.Text;
using API.MongoModel;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace API.Services
{
    public class OtpService
    {
        private readonly IMongoCollection<OtpRequest> _otps;
        private readonly Otp.OtpSettings _settings;

        public OtpService(IOptions<MongoDBSettings> mongoSettings, IOptions<Otp.OtpSettings> otpSettings)
        {
            _settings = otpSettings.Value;

            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);

            _otps = database.GetCollection<OtpRequest>("OtpRequests");

            CreateIndexes();
        }

        private void CreateIndexes()
        {
            // Index phone for fast lookup
            _otps.Indexes.CreateOne(new CreateIndexModel<OtpRequest>(
                Builders<OtpRequest>.IndexKeys.Ascending(x => x.Phone)));

            // TTL index on ExpiresAtUtc (Mongo will auto delete after expiry)
            // NOTE: TTL works on Date fields; Mongo deletes docs whose field value is older than now.
            // We set ExpiresAtUtc in the future; when that time passes, Mongo removes it.
            var ttlOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
            _otps.Indexes.CreateOne(new CreateIndexModel<OtpRequest>(
                Builders<OtpRequest>.IndexKeys.Ascending(x => x.ExpiresAtUtc), ttlOptions));
        }

        public static string NormalizePhone(string phone)
        {
            phone = (phone ?? "").Trim();
            // Keep + and digits only
            var filtered = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
            return filtered;
        }

        private string HashOtp(string phone, string otp)
        {
            // hash = SHA256(secret + phone + otp)
            var input = $"{_settings.SecretKey}|{phone}|{otp}";
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }

        private string GenerateOtp()
        {
            // 6-digit random (cryptographically secure)
            int max = (int)Math.Pow(10, _settings.CodeLength);
            int min = max / 10;

            var value = RandomNumberGenerator.GetInt32(min, max);
            return value.ToString().PadLeft(_settings.CodeLength, '0');
        }

        public async Task<(bool ok, string message, string? otpForDev)> SendAsync(string phone)
        {
            phone = NormalizePhone(phone);
            if (string.IsNullOrWhiteSpace(phone) || phone.Length < 8)
                return (false, "Invalid phone number.", null);

            var now = DateTime.UtcNow;

            // Rate limit: max sends per hour
            var oneHourAgo = now.AddHours(-1);
            var sendsLastHour = await _otps.CountDocumentsAsync(x =>
                x.Phone == phone && x.CreatedAtUtc >= oneHourAgo);

            if (sendsLastHour >= _settings.MaxSendsPerHour)
                return (false, "Too many OTP requests. Please try again later.", null);

            // Rate limit: min seconds between sends
            var last = await _otps.Find(x => x.Phone == phone)
                .SortByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync();

            if (last != null)
            {
                var seconds = (now - last.LastSentAtUtc).TotalSeconds;
                if (seconds < _settings.MinSecondsBetweenSends)
                {
                    var wait = Math.Ceiling(_settings.MinSecondsBetweenSends - seconds);
                    return (false, $"Please wait {wait} seconds before requesting another OTP.", null);
                }
            }

            var otp = GenerateOtp();
            var hash = HashOtp(phone, otp);

            var doc = new OtpRequest
            {
                Phone = phone,
                CodeHash = hash,
                CreatedAtUtc = now,
                LastSentAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(_settings.ExpiryMinutes),
                FailedAttempts = 0,
                UsedAtUtc = null
            };

            await _otps.InsertOneAsync(doc);

            // Later: integrate real SMS sending here (Twilio etc.)
            Console.WriteLine($"[OTP] Phone={phone} OTP={otp} (dev log)");

            // Return OTP only in dev/testing if enabled
            var otpForDev = _settings.ReturnOtpInResponse ? otp : null;
            return (true, "OTP sent.", otpForDev);
        }

        public async Task<(bool ok, string message)> VerifyAsync(string phone, string code)
        {
            phone = NormalizePhone(phone);
            code = (code ?? "").Trim();

            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(code))
                return (false, "Phone and OTP are required.");

            var now = DateTime.UtcNow;

            // Get latest OTP that is not used and not expired
            var otpDoc = await _otps.Find(x =>
                    x.Phone == phone &&
                    x.UsedAtUtc == null &&
                    x.ExpiresAtUtc > now)
                .SortByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync();

            if (otpDoc == null)
                return (false, "No valid OTP found. Please request a new OTP.");

            if (otpDoc.FailedAttempts >= _settings.MaxFailedAttempts)
                return (false, "Too many wrong attempts. Please request a new OTP.");

            var hash = HashOtp(phone, code);

            if (!string.Equals(hash, otpDoc.CodeHash, StringComparison.OrdinalIgnoreCase))
            {
                // increment failed attempts
                var updateFail = Builders<OtpRequest>.Update.Inc(x => x.FailedAttempts, 1);
                await _otps.UpdateOneAsync(x => x.Id == otpDoc.Id, updateFail);

                return (false, "Invalid OTP.");
            }

            // Mark as used
            var updateUsed = Builders<OtpRequest>.Update.Set(x => x.UsedAtUtc, now);
            await _otps.UpdateOneAsync(x => x.Id == otpDoc.Id, updateUsed);

            return (true, "OTP verified.");
        }
    }
}
