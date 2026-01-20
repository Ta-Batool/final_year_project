namespace API.Otp
{
    public class OtpSettings
    {
        public string SecretKey { get; set; } = "CHANGE_ME";
        public int CodeLength { get; set; } = 6;
        public int ExpiryMinutes { get; set; } = 5;

        public int MinSecondsBetweenSends { get; set; } = 60;
        public int MaxSendsPerHour { get; set; } = 5;

        public int MaxFailedAttempts { get; set; } = 5;

        // If true (dev), API returns OTP in response for testing
        public bool ReturnOtpInResponse { get; set; } = true;
    }
}
