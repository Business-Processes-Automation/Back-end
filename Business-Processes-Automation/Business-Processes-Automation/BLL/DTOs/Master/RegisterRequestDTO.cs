using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.BLL.DTOs.Master;

public class RegisterRequestDTO
{
    [Required(ErrorMessage = "Ім'я обов'язкове.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Ім'я має містити від 1 до 100 символів.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Прізвище обов'язкове.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Прізвище має містити від 1 до 100 символів.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Ім'я користувача обов'язкове.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Ім'я користувача має містити від 3 до 50 символів.")]
    [RegularExpression(
        @"^[a-zA-Z0-9_]+$",
        ErrorMessage = "Ім'я користувача: лише латинські літери, цифри та _.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Email обов'язковий.")]
    [EmailAddress(ErrorMessage = "Вкажіть коректний email.")]
    [StringLength(256, ErrorMessage = "Email не може перевищувати 256 символів.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Номер телефону обов'язковий.")]
    [Phone(ErrorMessage = "Вкажіть коректний номер телефону.")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Номер телефону має містити від 5 до 20 символів.")]
    [RegularExpression(
        @"^\+?[0-9\s\-()]{5,20}$",
        ErrorMessage = "Номер телефону: лише цифри та символи + - ( ).")]
    public string PhoneNumber { get; set; } = null!;

    [StringLength(64, MinimumLength = 1, ErrorMessage = "Часовий пояс має містити від 1 до 64 символів.")]
    public string TimeZone { get; set; } = "UTC";

    [Required(ErrorMessage = "Пароль обов'язковий.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Пароль має містити від 8 до 128 символів.")]
    public string Password { get; set; } = null!;
}
