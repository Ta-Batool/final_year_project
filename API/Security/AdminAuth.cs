using Microsoft.Extensions.Configuration;
using System.Text;

namespace API.Security
{
    public class AdminAuth
    {
        private readonly string _email;
        private readonly string _password;

        public AdminAuth(IConfiguration config)
        {
            _email = Environment.GetEnvironmentVariable("ADMIN_EMAIL")
                     ?? config["AdminAuth:Email"]
                     ?? "admin@admin.com";

            _password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                        ?? config["AdminAuth:Password"]
                        ?? "admin123";
        }

        public bool IsAdmin(HttpRequest request)
        {
            if (!request.Headers.TryGetValue("Authorization", out var auth)) return false;

            var header = auth.ToString();
            if (!header.StartsWith("Basic ")) return false;

            var encoded = header.Substring("Basic ".Length).Trim();

            string decoded;
            try
            {
                decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            }
            catch
            {
                return false;
            }

            var parts = decoded.Split(':', 2);
            if (parts.Length != 2) return false;

            var email = parts[0];
            var pass = parts[1];

            return email == _email && pass == _password;
        }
    }
}
