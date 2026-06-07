using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.BLL.DTOs.Schedule.Appointments;

public class RescheduleAppointmentRequestDTO
{
    [Required(ErrorMessage = "Новий час початку обов'язковий.")]
    public DateTime StartLocal { get; set; }
}
