using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.DAL.Entities;

public class Payment : BaseEntity
{
    public int AppointmentId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public PaymentType PaymentType { get; set; } = PaymentType.Prepayment;
    public DateTime PaidAt { get; set; }

    // Navigation property
    public Appointment Appointment { get; set; } = null!;
}
