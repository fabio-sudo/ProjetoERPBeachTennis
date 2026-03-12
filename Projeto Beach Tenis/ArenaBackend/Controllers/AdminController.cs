using ArenaBackend.Data;
using ArenaBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ArenaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")] // Only admins can access these
    public class AdminController : ControllerBase
    {
        private readonly ArenaDbContext _context;

        public AdminController(ArenaDbContext context)
        {
            _context = context;
        }

        // --- USERS ---
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Include(u => u.Role).Select(u => new
            {
                u.Id,
                u.Name,
                u.Username,
                u.Email,
                u.RoleId,
                RoleName = u.Role != null ? u.Role.Name : null,
                u.Active
            }).ToListAsync();
            return Ok(users);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Usuário não encontrado");

            user.Name = dto.Name ?? user.Name;
            user.Username = dto.Username ?? user.Username;
            user.Email = dto.Email ?? user.Email;
            if (dto.RoleId.HasValue) user.RoleId = dto.RoleId.Value;
            if (dto.Active.HasValue) user.Active = dto.Active.Value;

            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Usuário atualizado." });
        }

        // --- ROLES & PERMISSIONS ---
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission).ToListAsync();
            return Ok(roles.Select(r => new
            {
                r.Id,
                r.Name,
                Permissions = r.RolePermissions.Select(rp => rp.Permission?.Name).ToList()
            }));
        }

        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new { message = "O nome do perfil é obrigatório." });

            if (await _context.Roles.AnyAsync(r => r.Name == dto.Name))
            {
                return BadRequest(new { message = "Já existe um perfil com esse nome." });
            }

            var role = new Role { Name = dto.Name };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Perfil criado com sucesso.", id = role.Id, name = role.Name });
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetPermissions()
        {
            return Ok(await _context.Permissions.ToListAsync());
        }

        [HttpPost("roles/{roleId}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] List<int> permissionIds)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return NotFound("Perfil não encontrado");

            // Remove existing
            var existing = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            _context.RolePermissions.RemoveRange(existing);

            // Add new
            foreach (var pId in permissionIds)
            {
                var p = await _context.Permissions.FindAsync(pId);
                if (p != null)
                {
                    _context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pId });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Permissões atualizadas com sucesso." });
        }
    }

    public class CreateRoleDto
    {
        public required string Name { get; set; }
    }

    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? RoleId { get; set; }
        public bool? Active { get; set; }
    }
}
