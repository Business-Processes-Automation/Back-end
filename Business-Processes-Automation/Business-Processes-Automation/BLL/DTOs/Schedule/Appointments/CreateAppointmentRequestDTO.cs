using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.BLL.DTOs.Schedule.Appointments;

public class CreateAppointmentRequestDTO
{
    [Required(ErrorMessage = "Послуга обов'язкова.")]
    [Range(1, int.MaxValue)]
    public int ServiceId { get; set; }

    [Required(ErrorMessage = "Час початку обов'язковий.")]
    public DateTime StartLocal { get; set; }

    [Required(ErrorMessage = "Ім'я клієнта обов'язкове.")]
    [MaxLength(200)]
    public string ClientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Телефон клієнта обов'язковий.")]
    [MaxLength(20)]
    [Phone(ErrorMessage = "Невірний формат телефону.")]
    public string ClientPhone { get; set; } = string.Empty;
}
