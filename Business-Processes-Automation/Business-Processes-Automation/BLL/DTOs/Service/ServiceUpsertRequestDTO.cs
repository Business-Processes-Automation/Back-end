using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.BLL.DTOs.Service;

public abstract class ServiceUpsertRequestDTO : IValidatableObject
{
    [Required(ErrorMessage = "Назва послуги обов'язкова.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Назва має містити від 2 до 200 символів.")]
    public string ServiceName { get; set; } = null!;

    [Required(ErrorMessage = "Тривалість обов'язкова.")]
    [Range(1, 1440, ErrorMessage = "Тривалість має бути від 1 до 1440 хвилин.")]
    public int DurationInMinutes { get; set; }

    [Required(ErrorMessage = "Ціна обов'язкова.")]
    [Range(typeof(decimal), "0", "999999.99", ErrorMessage = "Ціна має бути від 0 до 999999.99 грн.")]
    public decimal Price { get; set; }

    [Range(typeof(decimal), "0", "999999.99", ErrorMessage = "Передоплата має бути від 0 до 999999.99 грн.")]
    public decimal Prepayment { get; set; }

    [Range(0, 480, ErrorMessage = "Підготовка до має бути від 0 до 480 хвилин.")]
    public int PreparationBeforeInMinutes { get; set; }

    [Range(0, 480, ErrorMessage = "Підготовка після має бути від 0 до 480 хвилин.")]
    public int PreparationAfterInMinutes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Prepayment > Price)
        {
            yield return new ValidationResult(
                "Передоплата не може перевищувати ціну послуги.",
                [nameof(Prepayment)]);
        }
    }
}
