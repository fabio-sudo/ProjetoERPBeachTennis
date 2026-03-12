using System.Text.Json.Serialization;

namespace ArenaBackend.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public required string Name { get; set; } // e.g., View_Dashboard, Manage_Users
        public required string Screen { get; set; } // e.g., Dashboard, PDV, Comandas

        [JsonIgnore]
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
