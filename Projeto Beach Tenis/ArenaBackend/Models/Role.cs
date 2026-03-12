using System.Text.Json.Serialization;

namespace ArenaBackend.Models
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; } // e.g., Administrador, Garçom

        [JsonIgnore]
        public ICollection<User> Users { get; set; } = new List<User>();

        [JsonIgnore]
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
