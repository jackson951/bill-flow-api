using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BillFlow.API.Models;
using BillFlow.API.DTOs;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly InvoiceProDbContext _context;

    public EmployeeController(InvoiceProDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee(EmployeeCreateDto dto)
    {
        if (await _context.Employees.AnyAsync(e => e.Email == dto.Email))
        {
            return BadRequest(new { error = "Email already in use." });
        }

        var userIdClaim = User.FindFirst("id");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            return Unauthorized(new { error = "Invalid or missing user ID in token." });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { error = "Admin user not found." });
        }

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            Role = dto.Role,
            Verified = false,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            CompanyName = user.CompanyName,
            Status = dto.Status,
            Permissions = dto.Permissions
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var response = new EmployeeResponseDto
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Role = employee.Role,
            Verified = employee.Verified,
            CreatedAt = employee.CreatedAt,
            Status = employee.Status,
            Permissions = employee.Permissions
        };

        return Ok(new { message = "Employee created.", employee = response });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        var response = new EmployeeResponseDto
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Role = employee.Role,
            Verified = employee.Verified,
            CreatedAt = employee.CreatedAt,
            Status = employee.Status,
            Permissions = employee.Permissions
        };

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEmployees()
    {
        var userIdClaim = User.FindFirst("id");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        var employees = await _context.Employees
            .Where(e => e.UserId == userId)
            .Select(e => new EmployeeResponseDto
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                Role = e.Role,
                Verified = e.Verified,
                CreatedAt = e.CreatedAt,
                Status = e.Status,
                Permissions = e.Permissions
            })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(Guid id, EmployeeCreateDto dto)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        employee.FullName = dto.FullName;
        employee.Email = dto.Email;
        employee.Role = dto.Role;
        employee.PasswordHash = HashPassword(dto.Password);
        employee.Status = dto.Status;
        employee.Permissions = dto.Permissions;

        await _context.SaveChangesAsync();

        return Ok("Employee updated.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return Ok("Employee deleted.");
    }

    // Helper method for hashing
    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
