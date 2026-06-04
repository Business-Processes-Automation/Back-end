namespace Business_Processes_Automation.BLL.DTOs.Master;

public class AuthResponseDTO
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string TimeZone { get; set; } = null!;
}
