namespace HotelManager.Application.DTOs.Reports;

public class PeriodReportDto
{
    public string Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
}
