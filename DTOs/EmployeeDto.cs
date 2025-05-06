namespace BillFlow.API.DTOs
{
    public class EmployeeCreateDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
        public string? Status { get; set; }
        public string? Permissions { get; set; }
    }

    public class EmployeeResponseDto
    {
        public Guid Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public bool Verified { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; }
        public string? Permissions { get; set; }
    }

}
