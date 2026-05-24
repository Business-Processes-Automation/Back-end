using System.ComponentModel.DataAnnotations;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.DAL.Entities;

public class Payment : BaseEntity
{
    public int AppointmentId { get; set; }

    [Range(typeof(decimal), "0.01", "999999.99")]
    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public PaymentType PaymentType { get; set; } = PaymentType.Prepayment;

    public DateTime? PaidAt { get; set; }

    public Appointment Appointment { get; set; } = null!;
}
