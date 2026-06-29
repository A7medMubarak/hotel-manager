using HotelManager.Application.Common;
using HotelManager.Application.DTOs.Reports;
using HotelManager.Application.Services.Interfaces;
using HotelManager.Domain.Enums;
using HotelManager.Domain.Interfaces;

namespace HotelManager.Application.Services;

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;

    public ReportService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DailyReportDto> GetDailyAsync(CancellationToken cancellationToken = default)
    {
        var businessDate = BusinessDateHelper.GetBusinessDate();
        var (windowStart, windowEnd) = BusinessDateHelper.GetBusinessDayWindow();

        var activeBookings = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Active
                && b.CheckIn <= businessDate
                && b.CheckOut > businessDate)
            .Include(b => b.Payments)
            .ToListAsync(cancellationToken);

        var newCheckIns = await _context.Bookings
            .CountAsync(b => b.CheckIn == businessDate, cancellationToken);

        var newCheckOuts = await _context.Bookings
            .CountAsync(b => b.CheckOut == businessDate, cancellationToken);

        var totalCollected = await _context.Payments
            .Where(p => p.CreatedAt >= windowStart && p.CreatedAt < windowEnd)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

        var theoreticalRevenue = activeBookings.Sum(b =>
            BookingCalculator.TotalCost(b.CheckIn, b.CheckOut, b.PricePerNight));

        var outstandingBalance = activeBookings.Sum(b =>
            BookingCalculator.Balance(b.CheckIn, b.CheckOut, b.PricePerNight, b.Payments));

        return new DailyReportDto
        {
            BusinessDate = businessDate,
            ActiveBookings = activeBookings.Count,
            NewCheckIns = newCheckIns,
            NewCheckOuts = newCheckOuts,
            TotalCollected = totalCollected,
            TheoreticalRevenue = theoreticalRevenue,
            OutstandingBalance = outstandingBalance
        };
    }

    public async Task<List<OutstandingBalanceDto>> GetOutstandingAsync(CancellationToken cancellationToken = default)
    {
        var bookings = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Active)
            .Include(b => b.Room)
            .Include(b => b.BookingGuests).ThenInclude(bg => bg.Guest)
            .Include(b => b.Payments)
            .ToListAsync(cancellationToken);

        return bookings
            .Select(b =>
            {
                var payments = b.Payments.ToList();
                var totalCost = BookingCalculator.TotalCost(b.CheckIn, b.CheckOut, b.PricePerNight);
                var totalPaid = BookingCalculator.TotalPaid(payments);
                var balance = totalCost - totalPaid;
                var primaryGuest = b.BookingGuests.FirstOrDefault(bg => bg.IsPrimary);

                return new OutstandingBalanceDto
                {
                    BookingId = b.Id,
                    RoomNumber = b.Room.Number,
                    GuestName = primaryGuest?.Guest.FullName ?? "N/A",
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    TotalCost = totalCost,
                    TotalPaid = totalPaid,
                    Balance = balance
                };
            })
            .Where(dto => dto.Balance > 0)
            .OrderByDescending(dto => dto.Balance)
            .ToList();
    }

    public async Task<PeriodReportDto> GetWeeklyAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var saturday = today.AddDays(-((int)today.DayOfWeek + 1) % 7);
        var friday = saturday.AddDays(6);

        return await GetPeriodReport(saturday, friday, "Weekly", cancellationToken);
    }

    public async Task<PeriodReportDto> GetMonthlyAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var firstDay = new DateOnly(today.Year, today.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        return await GetPeriodReport(firstDay, lastDay, "Monthly", cancellationToken);
    }

    private async Task<PeriodReportDto> GetPeriodReport(DateOnly startDate, DateOnly endDate, string period, CancellationToken cancellationToken = default)
    {
        var bookings = await _context.Bookings
            .Where(b => b.CheckIn < endDate.AddDays(1) && b.CheckOut > startDate)
            .Include(b => b.Payments)
            .ToListAsync(cancellationToken);

        var totalCollected = bookings
            .SelectMany(b => b.Payments)
            .Where(p => DateOnly.FromDateTime(p.PaymentDate) >= startDate
                     && DateOnly.FromDateTime(p.PaymentDate) <= endDate)
            .Sum(p => p.Amount);

        var totalRevenue = bookings
            .Sum(b => BookingCalculator.TotalCost(b.CheckIn, b.CheckOut, b.PricePerNight));

        return new PeriodReportDto
        {
            Period = period,
            StartDate = startDate,
            EndDate = endDate,
            TotalCollected = totalCollected,
            TotalRevenue = totalRevenue,
            TotalBookings = bookings.Count,
            CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed)
        };
    }
}
