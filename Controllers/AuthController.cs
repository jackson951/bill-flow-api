using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BillFlow.API.Models;
using BillFlow.API.DTOs;
using Google.Apis.Auth;

namespace BillFlow.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly InvoiceProDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(InvoiceProDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email is already registered.");

            var passwordHash = HashPassword(dto.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                CompanyName = dto.CompanyName,
                Role = "Admin",
                Verified = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || user.PasswordHash != HashPassword(dto.Password))
                return Unauthorized("Invalid email or password.");

            var token = GenerateJwtToken(user.Id.ToString(), user.Email, user.Role);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.CompanyName,
                    user.Role
                }
            });
        }

        [HttpPost("employee-login")]
        public async Task<IActionResult> EmployeeLogin(UserLoginDto dto)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Email == dto.Email);

            if (employee == null || employee.PasswordHash != HashPassword(dto.Password))
                return Unauthorized("Invalid email or password.");

            var token = GenerateJwtToken(employee.Id.ToString(), employee.Email, employee.Role);

            return Ok(new
            {
                token,
                employee = new
                {
                    employee.Id,
                    employee.FullName,
                    employee.Email,
                    employee.CompanyName,
                    employee.Role,
                    employee.Permissions,
                    employee.UserId // Employer's ID
                }
            });
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["Google:ClientId"] }
                });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = payload.Email,
                        FullName = payload.Name,
                        CompanyName = "",
                        Role = "Admin",
                        Verified = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                var token = GenerateJwtToken(user.Id.ToString(), user.Email, user.Role);

                return Ok(new
                {
                    token,
                    user = new
                    {
                        user.Id,
                        user.FullName,
                        user.Email,
                        user.CompanyName,
                        user.Role
                    }
                });
            }
            catch
            {
                return Unauthorized("Invalid Google token.");
            }
        }

        private string GenerateJwtToken(string id, string email, string role)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim("id", id),
                new Claim("role", role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
