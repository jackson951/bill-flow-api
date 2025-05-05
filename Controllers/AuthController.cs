using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using BillFlow.API.Models; // your scaffolded User model
using BillFlow.API.DTOs;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly InvoiceProDbContext _context;

    public AuthController(InvoiceProDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto dto)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest("Email is already registered.");
        }

        // Hash password
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
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Email and password are required.");
        }

        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var hashedInputPassword = HashPassword(dto.Password);
            if (user.PasswordHash != hashedInputPassword)
            {
                return Unauthorized("Invalid email or password.");
            }

            return Ok(new
            {
                message = "Login successful",
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
        catch (Exception ex)
        {
            // TEMPORARY: Show full error for debugging
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }


    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
