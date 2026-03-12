namespace ArenaBackend.DTOs
{
    public class EmployeeResponseDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EmployeeCreateDto
    {
        public required string Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int RoleId { get; set; }
    }

    public class EmployeeUpdateDto
    {
        public required string Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int RoleId { get; set; }
        public bool Active { get; set; }
    }
}
