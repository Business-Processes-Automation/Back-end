using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.DTOs.Schedule;

public class AppointmentDetailsResponseDTO
{
    public int Id { get; set; }

    public DateTime StartLocal { get; set; }

    public DateTime EndLocal { get; set; }

    public string ServiceName { get; set; } = null!;

    public int DurationInMinutes { get; set; }

    public string ClientName { get; set; } = null!;

    public string ClientPhone { get; set; } = null!;

    public AppointmentStatus Status { get; set; }

    public decimal PriceAtBooking { get; set; }

    public decimal PrepaymentAmount { get; set; }
}
