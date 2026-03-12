namespace ArenaBackend.DTOs
{
    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class RegisterRequest
    {
        public required string Name { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public int RoleId { get; set; }
    }

    public class AuthResponse
    {
        public required string Token { get; set; }
        public required string Name { get; set; }
        public required string Role { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class GoogleLoginRequest
    {
        public required string Credential { get; set; }
    }
}
