namespace BillFlow.API.DTOs
{
    public class CustomerResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string Type { get; set; } = null!;
    }
}
