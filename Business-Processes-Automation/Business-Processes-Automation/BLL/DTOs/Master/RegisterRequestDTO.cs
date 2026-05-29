namespace Business_Processes_Automation.BLL.DTOs.Master
{
    public class RegisterRequestDTO
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string TimeZone { get; set; } = "UTC";

        public string Password { get; set; } = null!;
    }
}
