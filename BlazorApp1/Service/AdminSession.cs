namespace BlazorApp1.Service
{
    public class AdminSession
    {
        public bool IsAdmin { get; private set; }
        public string? BasicAuthHeader { get; private set; }

        public void Login(string email, string password)
        {
            var raw = $"{email}:{password}";
            var base64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw));
            BasicAuthHeader = $"Basic {base64}";
            IsAdmin = true;
        }

        public void Logout()
        {
            IsAdmin = false;
            BasicAuthHeader = null;
        }
    }
}
