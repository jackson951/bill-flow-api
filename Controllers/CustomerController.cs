using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BillFlow.API.Models;
using BillFlow.API.DTOs;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly InvoiceProDbContext _context;

    public CustomerController(InvoiceProDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(CustomerCreateDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var customer = new Customer
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            Type = dto.Type,
            UserId = userId.Value
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Customer created.", id = customer.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var customers = await _context.Customers
            .Where(c => c.UserId == userId)
            .Select(c => new CustomerResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                Type = c.Type
            }).ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var customer = await _context.Customers
            .Where(c => c.UserId == userId && c.Id == id)
            .FirstOrDefaultAsync();

        if (customer == null) return NotFound();

        return Ok(new CustomerResponseDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            Type = customer.Type
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, CustomerCreateDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var customer = await _context.Customers
            .Where(c => c.UserId == userId && c.Id == id)
            .FirstOrDefaultAsync();

        if (customer == null) return NotFound();

        customer.Name = dto.Name;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;
        customer.Type = dto.Type;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Customer updated." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var customer = await _context.Customers
            .Where(c => c.UserId == userId && c.Id == id)
            .FirstOrDefaultAsync();

        if (customer == null) return NotFound();

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Customer deleted." });
    }

    private Guid? GetUserId()
    {
        var idClaim = User.FindFirst("id");
        return Guid.TryParse(idClaim?.Value, out var userId) ? userId : null;
    }
}
