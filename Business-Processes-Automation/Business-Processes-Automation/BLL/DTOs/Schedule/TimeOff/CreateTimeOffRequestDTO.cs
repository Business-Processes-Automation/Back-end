using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.BLL.DTOs.Schedule.TimeOff;

public class CreateTimeOffRequestDTO
{
    [Required(ErrorMessage = "Час початку обов'язковий.")]
    public DateTime StartLocal { get; set; }

    [Required(ErrorMessage = "Час закінчення обов'язковий.")]
    public DateTime EndLocal { get; set; }

    public bool IsFullDay { get; set; }
}
