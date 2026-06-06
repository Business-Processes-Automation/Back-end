using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.DAL.Entities;

public class Service : BaseEntity
{
    public int MasterId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ServiceName { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int DurationInMinutes { get; set; }

    [Range(typeof(decimal), "0", "999999.99")]
    public decimal Price { get; set; }

    [Range(typeof(decimal), "0", "999999.99")]
    public decimal Prepayment { get; set; }

    [Range(0, 480)]
    public int PreparationBeforeInMinutes { get; set; }

    [Range(0, 480)]
    public int PreparationAfterInMinutes { get; set; }

    /// <summary>
    /// Загальний час майстра: підготовка до + обслуговування + підготовка після.
    /// Обчислюється при створенні та оновленні послуги.
    /// </summary>
    [Range(1, 2400)]
    public int TotalOccupiedMinutes { get; set; }

    public Master Master { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
