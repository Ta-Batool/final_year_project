using API.Otp;
using Microsoft.Extensions.Options;
using PhoneNumbers;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace API.Services
{
    public class OtpService
    {
        private readonly TwilioSettings _twilio;
        private readonly PhoneNumberUtil _phoneUtil = PhoneNumberUtil.GetInstance();

        public OtpService(IOptions<TwilioSettings> twilioOptions)
        {
            _twilio = twilioOptions.Value;
            TwilioClient.Init(_twilio.AccountSid, _twilio.AuthToken);
        }

        public (bool ok, string message, string e164Phone) ValidatePhone(string countryIso, string phone)
        {
            try
            {
                var parsed = _phoneUtil.Parse(phone, countryIso);

                if (!_phoneUtil.IsValidNumber(parsed))
                    return (false, "Invalid phone number for selected country.", "");

                var e164 = _phoneUtil.Format(parsed, PhoneNumberFormat.E164);
                return (true, "Valid phone number.", e164);
            }
            catch
            {
                return (false, "Invalid phone number format.", "");
            }
        }

        public async Task<(bool ok, string message)> SendAsync(string countryIso, string phone)
        {
            var validation = ValidatePhone(countryIso, phone);

            if (!validation.ok)
                return (false, validation.message);

            await VerificationResource.CreateAsync(
                to: validation.e164Phone,
                channel: "sms",
                pathServiceSid: _twilio.VerifyServiceSid
            );

            return (true, "OTP sent successfully.");
        }

        public async Task<(bool ok, string message, string e164Phone)> VerifyAsync(string countryIso, string phone, string code)
        {
            var validation = ValidatePhone(countryIso, phone);

            if (!validation.ok)
                return (false, validation.message, "");

            var result = await VerificationCheckResource.CreateAsync(
                to: validation.e164Phone,
                code: code,
                pathServiceSid: _twilio.VerifyServiceSid
            );

            if (result.Status == "approved")
                return (true, "OTP verified successfully.", validation.e164Phone);

            return (false, "Invalid or expired OTP.", validation.e164Phone);
        }
    }
}