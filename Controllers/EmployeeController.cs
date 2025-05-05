using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Security.Claims;
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
            return BadRequest("Email already in use.");
        }

        var userIdClaim = User.FindFirst("id");
        if (userIdClaim == null)
            return Unauthorized("User ID not found in token.");

        Guid userId = Guid.Parse(userIdClaim.Value);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            Role = dto.Role,
            Verified = false,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Employee created." });
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
            CreatedAt = employee.CreatedAt
        };

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEmployees()
    {
        var userIdClaim = User.FindFirst("id");
        if (userIdClaim == null)
            return Unauthorized("User ID not found in token.");

        Guid userId = Guid.Parse(userIdClaim.Value);

        var employees = await _context.Employees
            .Where(e => e.UserId == userId)
            .Select(e => new EmployeeResponseDto
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                Role = e.Role,
                Verified = e.Verified,
                CreatedAt = e.CreatedAt
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

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
