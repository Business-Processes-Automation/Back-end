using System.ComponentModel.DataAnnotations;
using Business_Processes_Automation.BLL.Localization;

namespace Business_Processes_Automation.BLL.DTOs.Finance;

public class UpdateExpenseRequestDTO : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Невірний ідентифікатор категорії.")]
    public int? ExpenseCategoryId { get; set; }

    [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = "Сума має бути від 0.01 до 999999.99 грн.")]
    public decimal? Amount { get; set; }

    public DateOnly? DateOfExpense { get; set; }

    [MaxLength(1000, ErrorMessage = "Опис не може перевищувати 1000 символів.")]
    public string? Description { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ExpenseCategoryId is null
            && Amount is null
            && DateOfExpense is null
            && Description is null)
        {
            yield return new ValidationResult(
                MasterFinanceMessages.NothingToUpdate,
                [nameof(ExpenseCategoryId), nameof(Amount), nameof(DateOfExpense), nameof(Description)]);
        }
    }
}
