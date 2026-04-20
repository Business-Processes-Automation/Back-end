using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.DAL.Entities;

public class Appointment : BaseEntity
{
    public int ClientId { get; set; }
    public int ServiceId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Planned;
    public decimal PriceAtBooking { get; set; }
    public decimal PrepaymentAmount { get; set; }

    // Navigation property
    public Client Client { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
