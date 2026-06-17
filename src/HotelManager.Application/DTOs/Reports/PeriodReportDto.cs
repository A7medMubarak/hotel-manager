namespace HotelManager.Application.DTOs.Reports;

public class PeriodReportDto
{
    public string Period { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
}
