namespace Business_Processes_Automation.BLL.DTOs.Finance;

public class FinanceSummaryDTO
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public decimal Revenue { get; set; }

    public decimal Expenses { get; set; }

    public decimal Profit { get; set; }

    public int CompletedAppointmentsCount { get; set; }

    public int ExpensesCount { get; set; }
}
