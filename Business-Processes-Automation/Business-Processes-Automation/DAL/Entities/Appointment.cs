using System.ComponentModel.DataAnnotations;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.DAL.Entities;

public class Appointment : BaseEntity
{
    public int ClientId { get; set; }
    public int ServiceId { get; set; }

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Planned;

    [Range(0, 10)]
    public int RescheduleCount { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [Range(typeof(decimal), "0", "999999.99")]
    public decimal PriceAtBooking { get; set; }

    [Range(typeof(decimal), "0", "999999.99")]
    public decimal PrepaymentAmount { get; set; }

    public Client Client { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
