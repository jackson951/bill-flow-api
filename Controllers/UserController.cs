using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using BillFlow.API.Models;
using BillFlow.API.DTOs;
using Microsoft.AspNetCore.Authorization; // Add this import

namespace BillFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // Protect all actions in the controller with authorization
public class UserController : ControllerBase
{
    private readonly InvoiceProDbContext _context;

    public UserController(InvoiceProDbContext context)
    {
        _context = context;
    }

    // Get user profile
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var profile = new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            CompanyName = user.CompanyName,
            Role = user.Role,
            Verified = user.Verified,
            CreatedAt = user.CreatedAt
        };

        return Ok(profile);
    }

    // Update user profile
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfile(Guid id, UpdateUserProfileDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.FullName = dto.FullName;
        user.CompanyName = dto.CompanyName;

        await _context.SaveChangesAsync();
        return Ok("Profile updated.");
    }

    // Change password
    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(Guid id, ChangePasswordDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        if (user.PasswordHash != HashPassword(dto.CurrentPassword))
            return BadRequest("Incorrect current password.");

        user.PasswordHash = HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        return Ok("Password changed.");
    }

    // Forgot password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return NotFound("User not found.");

        // Generate and return token (simulated here)
        var token = Guid.NewGuid().ToString();
        // Simulate sending the token to email
        return Ok(new { Message = "Reset token generated.", Token = token });
    }

    // Reset password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return NotFound();

        // Token verification placeholder (you should store/verify real tokens)
        if (string.IsNullOrWhiteSpace(dto.Token))
            return BadRequest("Invalid token.");

        user.PasswordHash = HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        return Ok("Password reset successful.");
    }

    // Verify email
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return NotFound();

        if (dto.Token != "123456") // Replace with real token logic
            return BadRequest("Invalid verification token.");

        user.Verified = true;
        await _context.SaveChangesAsync();

        return Ok("Email verified successfully.");
    }

    // Get list of employees (Only admin users can access)
    [HttpGet("employees")]
    [Authorize(Roles = "Admin")]  // Add specific role-based authorization if needed
    public async Task<IActionResult> GetEmployees()
    {
        var employees = await _context.Users
            .Where(u => u.Role != "Admin")
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                CompanyName = u.CompanyName,
                Role = u.Role,
                Verified = u.Verified,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(employees);
    }

    // Delete a user
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only admin can delete users
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok("User deleted.");
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
