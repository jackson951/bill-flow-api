namespace BillFlow.API.DTOs
{
    public class UserRegisterDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // We'll hash this
        public string CompanyName { get; set; }
    }

}
