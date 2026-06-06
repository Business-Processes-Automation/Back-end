using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.BLL.DTOs.Schedule;

public class UpdateScheduleSettingsRequestDTO : IValidatableObject
{
    [Required(ErrorMessage = "Перерва між записами обов'язкова.")]
    [Range(0, 480, ErrorMessage = "Перерва має бути від 0 до 480 хвилин.")]
    public int BufferBetweenClientsMinutes { get; set; }

    [Required(ErrorMessage = "Крок слотів обов'язковий.")]
    [Range(5, 120, ErrorMessage = "Крок слотів: від 5 до 120 хвилин.")]
    public int FreeSlotIntervalMinutes { get; set; }

    [Required(ErrorMessage = "Мінімальний час до запису обов'язковий.")]
    [Range(0, 10080, ErrorMessage = "Мінімальний час до запису: від 0 до 10080 хвилин.")]
    public int MinBookingNoticeMinutes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FreeSlotIntervalMinutes % 5 != 0)
        {
            yield return new ValidationResult(
                "Крок слотів: від 5 до 120 хв, кратно 5 (наприклад, 15 або 30).",
                [nameof(FreeSlotIntervalMinutes)]);
        }
    }
}
