using System;
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
                }
                else
                {
                    IsAdmin = false;
                    Email = null;
                }
            }
            catch
            {
                IsAdmin = false;
                Email = null;
            }
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            // default creds
            if (email == "admin@admin.com" && password == "admin123")
            {
                IsAdmin = true;
                Email = email;

                await _storage.SetAsync(Key, new AdminAuthState
                {
                    IsAdmin = true,
                    Email = email,
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
            await _storage.DeleteAsync(Key);
        }

        private class AdminAuthState
        {
            public bool IsAdmin { get; set; }
            public string? Email { get; set; }
            public DateTime LoggedInAtUtc { get; set; }
        }
    }
}
