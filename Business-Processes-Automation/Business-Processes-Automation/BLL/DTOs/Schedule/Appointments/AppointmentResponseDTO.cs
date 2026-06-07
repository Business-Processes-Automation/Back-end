using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.DTOs.Schedule.Appointments;

public class AppointmentResponseDTO
{
    public int Id { get; set; }

    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public int DurationInMinutes { get; set; }

    public int TotalOccupiedMinutes { get; set; }

    public int ClientId { get; set; }

    public string ClientName { get; set; } = null!;

    public string ClientPhone { get; set; } = null!;

    public DateTime StartLocal { get; set; }

    public DateTime EndLocal { get; set; }

    public AppointmentStatus Status { get; set; }

    public AppointmentStatus DisplayStatus { get; set; }

    public int RescheduleCount { get; set; }

    public string? Notes { get; set; }

    public decimal PriceAtBooking { get; set; }

    public decimal PrepaymentAmount { get; set; }
}
