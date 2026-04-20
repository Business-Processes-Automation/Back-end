namespace Business_Processes_Automation.DAL.Entities;

public class Service : BaseEntity
{
    public int MasterId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int DurationInMinutes { get; set; }
    public decimal Price { get; set; }
    public decimal Prepayment { get; set; }
    public int PreparationBeforeInMinutes { get; set; }
    public int PreparationAfterInMinutes { get; set; }

    // Navigation property
    public Master Master { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
