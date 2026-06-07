using Business_Processes_Automation.BLL.DTOs.Schedule.TimeOff;

namespace Business_Processes_Automation.BLL.DTOs.Schedule.Calendar;

public class CalendarDayResponseDTO
{
    public DateOnly Date { get; set; }

    public CalendarWorkingHoursDTO? WorkingHours { get; set; }

    public IReadOnlyList<TimeOffResponseDTO> TimeOffs { get; set; } = [];

    public IReadOnlyList<CalendarAppointmentSummaryDTO> Appointments { get; set; } = [];
}
