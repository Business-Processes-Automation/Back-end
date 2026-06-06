namespace Business_Processes_Automation.BLL.DTOs.Service;

public class ServiceResponseDTO
{
    public int Id { get; set; }

    public string ServiceName { get; set; } = null!;

    public int DurationInMinutes { get; set; }

    public decimal Price { get; set; }

    public decimal Prepayment { get; set; }

    public int PreparationBeforeInMinutes { get; set; }

    public int PreparationAfterInMinutes { get; set; }

    /// <summary>
    /// Загальний час майстра: підготовка до + обслуговування + підготовка після.
    /// </summary>
    public int TotalOccupiedMinutes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
