namespace ArenaBackend.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }

        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
