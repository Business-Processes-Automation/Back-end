using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.BLL.DTOs.Master;

public class LoginRequestDTO
{
    [Required(ErrorMessage = "Email обов'язковий.")]
    [EmailAddress(ErrorMessage = "Вкажіть коректний email.")]
    [StringLength(256, ErrorMessage = "Email не може перевищувати 256 символів.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Пароль обов'язковий.")]
    [StringLength(128, MinimumLength = 1, ErrorMessage = "Пароль не може бути порожнім.")]
    public string Password { get; set; } = null!;
}
