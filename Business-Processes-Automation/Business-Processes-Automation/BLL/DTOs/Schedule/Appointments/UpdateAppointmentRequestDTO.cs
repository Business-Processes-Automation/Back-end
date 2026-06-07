using System.ComponentModel.DataAnnotations;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.DTOs.Schedule.Appointments;

public class UpdateAppointmentRequestDTO : IValidatableObject
{
    public AppointmentStatus? Status { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [Range(1, int.MaxValue)]
    public int? ServiceId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status is null && Notes is null && ServiceId is null)
        {
            yield return new ValidationResult(
                MasterAppointmentMessages.NothingToUpdate,
                [nameof(Status), nameof(Notes), nameof(ServiceId)]);
        }
    }
}
