namespace BillFlow.API.DTOs
{
    public class EmployeeCreateDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }  // Will be hashed
        public string Role { get; set; }
    }

    public class EmployeeResponseDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool Verified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
