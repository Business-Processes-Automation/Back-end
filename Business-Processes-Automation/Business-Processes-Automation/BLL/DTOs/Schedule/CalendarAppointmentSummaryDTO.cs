using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.DTOs.Schedule;

public class CalendarAppointmentSummaryDTO
{
    public int Id { get; set; }

    public DateTime StartLocal { get; set; }

    public DateTime EndLocal { get; set; }

    public string ServiceName { get; set; } = null!;

    public string ClientName { get; set; } = null!;

    public AppointmentStatus Status { get; set; }
}
