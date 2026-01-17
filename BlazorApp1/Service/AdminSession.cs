using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BlazorApp1.Service
{
    public class AdminSession
    {
        private const string Key = "admin_auth_v1";
        private readonly ProtectedLocalStorage _storage;

        public bool IsAdmin { get; private set; }
        public string? Email { get; private set; }

        // ✅ REQUIRED by AdminApiClient.cs
        public string? BasicAuthHeader { get; private set; }

        public AdminSession(ProtectedLocalStorage storage)
        {
            _storage = storage;
        }

        public async Task LoadAsync()
        {
            try
            {
                var result = await _storage.GetAsync<AdminAuthState>(Key);
                if (result.Success && result.Value is not null)
                {
                    IsAdmin = result.Value.IsAdmin;
                    Email = result.Value.Email;
                    BasicAuthHeader = result.Value.BasicAuthHeader;
                }
                else
                {
                    IsAdmin = false;
                    Email = null;
                    BasicAuthHeader = null;
                }
            }
            catch
            {
                IsAdmin = false;
                Email = null;
                BasicAuthHeader = null;
            }
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            // default creds
            if (email == "admin@admin.com" && password == "admin123")
            {
                IsAdmin = true;
                Email = email;

                // ✅ build Basic auth header (AdminApiClient uses this)
                var raw = $"{email}:{password}";
                BasicAuthHeader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));

                await _storage.SetAsync(Key, new AdminAuthState
                {
                    IsAdmin = true,
                    Email = email,
                    BasicAuthHeader = BasicAuthHeader,
                    LoggedInAtUtc = DateTime.UtcNow
                });

                return true;
            }

            return false;
        }

        public async Task LogoutAsync()
        {
            IsAdmin = false;
            Email = null;
            BasicAuthHeader = null;
            await _storage.DeleteAsync(Key);
        }

        private class AdminAuthState
        {
            public bool IsAdmin { get; set; }
            public string? Email { get; set; }
            public string? BasicAuthHeader { get; set; }
            public DateTime LoggedInAtUtc { get; set; }
        }
    }
}
