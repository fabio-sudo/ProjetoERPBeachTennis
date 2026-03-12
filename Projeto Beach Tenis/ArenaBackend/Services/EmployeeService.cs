using ArenaBackend.Data;
using ArenaBackend.DTOs;
using ArenaBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace ArenaBackend.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ArenaDbContext _context;

        public EmployeeService(ArenaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetAllAsync()
        {
            return await _context.Employees
                .Include(e => e.Role)
                .Select(e => new EmployeeResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Phone = e.Phone,
                    Email = e.Email,
                    Username = _context.Users.FirstOrDefault(u => u.Name == e.Name && u.RoleId == e.RoleId) != null ? _context.Users.FirstOrDefault(u => u.Name == e.Name && u.RoleId == e.RoleId)!.Username : null,
                    RoleId = e.RoleId,
                    RoleName = e.Role != null ? e.Role.Name : null,
                    Active = e.Active,
                    CreatedAt = e.CreatedAt
                }).ToListAsync();
        }

        public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
        {
            var e = await _context.Employees
                .Include(emp => emp.Role)
                .FirstOrDefaultAsync(emp => emp.Id == id);

            if (e == null) return null;

            return new EmployeeResponseDto
            {
                Id = e.Id,
                Name = e.Name,
                Phone = e.Phone,
                Email = e.Email,
                Username = _context.Users.FirstOrDefault(u => u.Name == e.Name && u.RoleId == e.RoleId) != null ? _context.Users.FirstOrDefault(u => u.Name == e.Name && u.RoleId == e.RoleId)!.Username : null,
                RoleId = e.RoleId,
                RoleName = e.Role != null ? e.Role.Name : null,
                Active = e.Active,
                CreatedAt = e.CreatedAt
            };
        }

        public async Task<EmployeeResponseDto> CreateAsync(EmployeeCreateDto dto)
        {
            var employee = new Employee
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                RoleId = dto.RoleId,
                Active = true,
                CreatedAt = DateTime.Now
            };

            _context.Employees.Add(employee);

            if (!string.IsNullOrEmpty(dto.Username) && !string.IsNullOrEmpty(dto.Password))
            {
                var role = await _context.Roles.FindAsync(dto.RoleId);
                if (role != null)
                {
                    _context.Users.Add(new User
                    {
                        Name = dto.Name,
                        Username = dto.Username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                        RoleId = dto.RoleId,
                        Role = role,
                        Active = true
                    });
                }
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(employee.Id) ?? throw new Exception("Falha ao criar funcionário.");
        }

        public async Task<EmployeeResponseDto?> UpdateAsync(int id, EmployeeUpdateDto dto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return null;

            employee.Name = dto.Name;
            employee.Phone = dto.Phone;
            employee.Email = dto.Email;
            employee.RoleId = dto.RoleId;
            employee.Active = dto.Active;

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Name == employee.Name && u.RoleId == employee.RoleId);
            if (existingUser != null)
            {
                if (!string.IsNullOrEmpty(dto.Username)) existingUser.Username = dto.Username;
                if (!string.IsNullOrEmpty(dto.Password)) existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                existingUser.Active = dto.Active;
                existingUser.RoleId = dto.RoleId;
            }
            else if (!string.IsNullOrEmpty(dto.Username) && !string.IsNullOrEmpty(dto.Password))
            {
                var role = await _context.Roles.FindAsync(dto.RoleId);
                if (role != null)
                {
                    _context.Users.Add(new User
                    {
                        Name = dto.Name,
                        Username = dto.Username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                        RoleId = dto.RoleId,
                        Role = role,
                        Active = dto.Active
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Fallback inside update
            }

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == employee.Name && u.RoleId == employee.RoleId);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
