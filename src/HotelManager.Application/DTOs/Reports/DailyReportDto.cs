namespace HotelManager.Application.DTOs.Reports;

public class DailyReportDto
{
    public DateOnly BusinessDate { get; set; }
    public int ActiveBookings { get; set; }
    public int NewCheckIns { get; set; }
    public int NewCheckOuts { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TheoreticalRevenue { get; set; }
    public decimal OutstandingBalance { get; set; }
}
