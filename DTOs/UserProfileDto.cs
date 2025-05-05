namespace BillFlow.API.DTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string Role { get; set; }
        public bool Verified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
