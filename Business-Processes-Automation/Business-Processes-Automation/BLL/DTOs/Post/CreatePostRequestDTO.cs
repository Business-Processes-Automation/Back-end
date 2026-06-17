using System.ComponentModel.DataAnnotations;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.DTOs.Post;

public class CreatePostRequestDTO : IValidatableObject
{
    [Required(ErrorMessage = "MasterId обов'язковий.")]
    [Range(1, int.MaxValue, ErrorMessage = "MasterId має бути додатним числом.")]
    public int MasterId { get; set; }

    [Required(ErrorMessage = "Текст поста обов'язковий.")]
    [StringLength(4000, MinimumLength = 1, ErrorMessage = "Текст має містити від 1 до 4000 символів.")]
    public string Text { get; set; } = string.Empty;

    [StringLength(2048, ErrorMessage = "URL зображення не може перевищувати 2048 символів.")]
    [Url(ErrorMessage = "Некоректний URL зображення.")]
    public string? ImageUrl { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Draft;

    public DateTime? ScheduledAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status == PostStatus.Scheduled && ScheduledAt is null)
        {
            yield return new ValidationResult(
                "Для запланованого поста необхідно вказати ScheduledAt.",
                [nameof(ScheduledAt)]);
        }

        if (Status == PostStatus.Scheduled && ScheduledAt <= DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "ScheduledAt має бути в майбутньому.",
                [nameof(ScheduledAt)]);
        }
    }
}
