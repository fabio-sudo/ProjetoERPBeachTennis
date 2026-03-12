using ArenaBackend.Data;
using ArenaBackend.DTOs;
using ArenaBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArenaBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly ArenaDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ArenaDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Active);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciais inválidas");
            }

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request)
        {
            Google.Apis.Auth.GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { _configuration["GoogleAuth:ClientId"] }
                };
                payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(request.Credential, settings);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException($"Login com Google falhou: {ex.Message}");
            }

            var email = payload.Email;
            if (string.IsNullOrEmpty(email)) throw new UnauthorizedAccessException("E-mail não fornecido pelo Google.");

            // Match by Email or Username (as an email)
            var user = await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Active && (u.Email == email || u.Username == email));

            if (user == null)
            {
                throw new UnauthorizedAccessException("E-mail não cadastrado na Arena. Por favor, contate o administrador.");
            }

            // Successfully matched the identity cryptographically. Emit our standard JWT.
            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new InvalidOperationException("Nome de usuário já existe");
            }

            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == request.RoleId);

            if (role == null) throw new InvalidOperationException("Função inválida");

            var user = new User
            {
                Name = request.Name,
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = request.RoleId,
                Active = true,
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponse> GetMeAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) throw new UnauthorizedAccessException("Usuário não encontrado");

            return GenerateAuthResponse(user);
        }

        private AuthResponse GenerateAuthResponse(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForBeachTennisAndFutvoleiAppDontShare");

            var permissions = user.Role?.RolePermissions.Select(rp => rp.Permission!.Screen).Distinct().ToList() ?? new List<string>();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User")
            };

            foreach (var perm in permissions)
            {
                claims.Add(new Claim("Permission", perm));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "1440")),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponse
            {
                Token = tokenHandler.WriteToken(token),
                Name = user.Name,
                Role = user.Role?.Name ?? "User",
                Permissions = permissions
            };
        }
    }
}
