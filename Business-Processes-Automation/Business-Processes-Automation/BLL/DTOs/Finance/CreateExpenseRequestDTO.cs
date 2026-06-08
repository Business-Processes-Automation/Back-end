using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.BLL.DTOs.Finance;

public class CreateExpenseRequestDTO
{
    [Required(ErrorMessage = "Категорія витрат обов'язкова.")]
    [Range(1, int.MaxValue, ErrorMessage = "Категорія витрат обов'язкова.")]
    public int ExpenseCategoryId { get; set; }

    [Required(ErrorMessage = "Сума обов'язкова.")]
    [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = "Сума має бути від 0.01 до 999999.99 грн.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Дата витрати обов'язкова.")]
    public DateOnly DateOfExpense { get; set; }

    [MaxLength(1000, ErrorMessage = "Опис не може перевищувати 1000 символів.")]
    public string? Description { get; set; }
}
