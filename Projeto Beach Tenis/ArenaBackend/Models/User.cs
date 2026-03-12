using System.Text.Json.Serialization;

namespace ArenaBackend.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Username { get; set; }
        public string? Email { get; set; }

        [JsonIgnore]
        public required string PasswordHash { get; set; }

        public int RoleId { get; set; }
        public bool Active { get; set; } = true;

        public Role? Role { get; set; }
    }
}
