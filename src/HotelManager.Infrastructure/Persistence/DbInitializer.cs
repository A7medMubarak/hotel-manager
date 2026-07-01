using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HotelManager.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        logger.LogInformation("Ensuring database is created...");
        await context.Database.EnsureCreatedAsync();

        if (!context.Users.Any())
        {
            var defaultPassword = Environment.GetEnvironmentVariable("DefaultAdminPassword") ?? "Admin123!";

            logger.LogInformation("No users found. Creating default accounts...");

            var owner = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword),
                Role = UserRole.Owner,
                CreatedAt = DateTime.UtcNow
            };

            var employee = new User
            {
                Username = "employee",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee123!"),
                Role = UserRole.Employee,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(owner, employee);
            await context.SaveChangesAsync();

            logger.LogInformation("Default accounts created: admin (Owner), employee (Employee)");
            logger.LogWarning("IMPORTANT: Change the default passwords immediately after first login.");
        }

        if (!context.Rooms.Any())
        {
            logger.LogInformation("Seeding demo data...");

            var today = DateOnly.FromDateTime(DateTime.Today);

            var rooms = new List<Room>
            {
                new() { Number = "101", Floor = 1, BedCount = 1, BathroomType = BathroomType.Ensuite, BasePricePerNight = 200 },
                new() { Number = "102", Floor = 1, BedCount = 1, BathroomType = BathroomType.Shared, BasePricePerNight = 150 },
                new() { Number = "201", Floor = 2, BedCount = 2, BathroomType = BathroomType.Ensuite, BasePricePerNight = 300 },
                new() { Number = "202", Floor = 2, BedCount = 2, BathroomType = BathroomType.Ensuite, BasePricePerNight = 350 },
                new() { Number = "301", Floor = 3, BedCount = 3, BathroomType = BathroomType.Ensuite, BasePricePerNight = 500 },
                new() { Number = "302", Floor = 3, BedCount = 1, BathroomType = BathroomType.Shared, BasePricePerNight = 250, IsUnderMaintenance = true }
            };

            context.Rooms.AddRange(rooms);
            await context.SaveChangesAsync();

            var guests = new List<Guest>
            {
                new() { FullName = "أحمد حسن", NationalId = "29801011234567", Address = "القاهرة، مصر", Phone = "01001234567", CreatedAt = DateTime.UtcNow },
                new() { FullName = "Sara Mohamed", NationalId = "29802021234568", Address = "Alexandria, Egypt", Phone = "01002345678", CreatedAt = DateTime.UtcNow },
                new() { FullName = "Mohamed Ali", NationalId = "29803031234569", Address = "Giza, Egypt", Phone = "01003456789", CreatedAt = DateTime.UtcNow },
                new() { FullName = "Fatima Omar", NationalId = "29804041234570", Address = "Luxor, Egypt", Phone = "01004567890", CreatedAt = DateTime.UtcNow },
                new() { FullName = "Khaled Ibrahim", NationalId = "29805051234571", Address = "Aswan, Egypt", Phone = "01005678901", CreatedAt = DateTime.UtcNow }
            };

            context.Guests.AddRange(guests);
            await context.SaveChangesAsync();

            var bookings = new List<Booking>
            {
                new()
                {
                    RoomId = rooms[0].Id,
                    CheckIn = today.AddDays(-2),
                    CheckOut = today.AddDays(3),
                    PricePerNight = rooms[0].BasePricePerNight,
                    Status = BookingStatus.Active,
                    Notes = "طلب إفطار",
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = 1
                },
                new()
                {
                    RoomId = rooms[2].Id,
                    CheckIn = today,
                    CheckOut = today.AddDays(5),
                    PricePerNight = rooms[2].BasePricePerNight,
                    Status = BookingStatus.Active,
                    Notes = "Quiet room requested",
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = 1
                },
                new()
                {
                    RoomId = rooms[1].Id,
                    CheckIn = today.AddDays(-10),
                    CheckOut = today.AddDays(-7),
                    PricePerNight = rooms[1].BasePricePerNight,
                    Status = BookingStatus.Completed,
                    Notes = null,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    CreatedByUserId = 1
                },
                new()
                {
                    RoomId = rooms[3].Id,
                    CheckIn = today.AddDays(-14),
                    CheckOut = today.AddDays(-10),
                    PricePerNight = rooms[3].BasePricePerNight,
                    Status = BookingStatus.Completed,
                    Notes = "Late checkout requested",
                    CreatedAt = DateTime.UtcNow.AddDays(-14),
                    CreatedByUserId = 1
                },
                new()
                {
                    RoomId = rooms[4].Id,
                    CheckIn = today.AddDays(5),
                    CheckOut = today.AddDays(10),
                    PricePerNight = rooms[4].BasePricePerNight,
                    Status = BookingStatus.Cancelled,
                    Notes = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = 1
                },
                new()
                {
                    RoomId = rooms[4].Id,
                    CheckIn = today.AddDays(1),
                    CheckOut = today.AddDays(4),
                    PricePerNight = rooms[4].BasePricePerNight,
                    Status = BookingStatus.Active,
                    Notes = "Honeymoon suite",
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = 1
                }
            };

            context.Bookings.AddRange(bookings);
            await context.SaveChangesAsync();

            var bookingGuests = new List<BookingGuest>
            {
                new() { BookingId = bookings[0].Id, GuestId = guests[0].Id, IsPrimary = true },
                new() { BookingId = bookings[1].Id, GuestId = guests[1].Id, IsPrimary = true },
                new() { BookingId = bookings[2].Id, GuestId = guests[2].Id, IsPrimary = true },
                new() { BookingId = bookings[3].Id, GuestId = guests[3].Id, IsPrimary = true },
                new() { BookingId = bookings[4].Id, GuestId = guests[4].Id, IsPrimary = true },
                new() { BookingId = bookings[5].Id, GuestId = guests[1].Id, IsPrimary = true },
                new() { BookingId = bookings[5].Id, GuestId = guests[4].Id, IsPrimary = false }
            };

            context.BookingGuests.AddRange(bookingGuests);
            await context.SaveChangesAsync();

            var payments = new List<Payment>
            {
                new() { BookingId = bookings[0].Id, Amount = 300, PaymentDate = DateTime.UtcNow.AddDays(-2), Notes = "دفعة مقدمة", CreatedAt = DateTime.UtcNow.AddDays(-2), CreatedByUserId = 1 },
                new() { BookingId = bookings[1].Id, Amount = 1500, PaymentDate = DateTime.UtcNow, Notes = "Full payment", CreatedAt = DateTime.UtcNow, CreatedByUserId = 1 },
                new() { BookingId = bookings[2].Id, Amount = 450, PaymentDate = DateTime.UtcNow.AddDays(-10), Notes = "Full payment", CreatedAt = DateTime.UtcNow.AddDays(-10), CreatedByUserId = 1 },
                new() { BookingId = bookings[3].Id, Amount = 700, PaymentDate = DateTime.UtcNow.AddDays(-14), Notes = "Partial payment", CreatedAt = DateTime.UtcNow.AddDays(-14), CreatedByUserId = 1 }
            };

            context.Payments.AddRange(payments);
            await context.SaveChangesAsync();

            logger.LogInformation("Demo data seeded successfully: {RoomCount} rooms, {GuestCount} guests, {BookingCount} bookings, {PaymentCount} payments.",
                rooms.Count, guests.Count, bookings.Count, payments.Count);
        }
    }
}
