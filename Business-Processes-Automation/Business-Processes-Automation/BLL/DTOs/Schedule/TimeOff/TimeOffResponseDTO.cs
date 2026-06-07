namespace Business_Processes_Automation.BLL.DTOs.Schedule.TimeOff;

public class TimeOffResponseDTO
{
    public int Id { get; set; }

    public DateTime StartLocal { get; set; }

    public DateTime EndLocal { get; set; }

    public bool IsFullDay { get; set; }
}
