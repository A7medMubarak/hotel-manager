using HotelManager.Domain.Entities;
using HotelManager.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Application.Tests.TestCommon;

public static class MockDbContext
{
    public static IApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    public static IApplicationDbContext CreateWithData(
        List<Room>? rooms = null,
        List<Guest>? guests = null,
        List<Booking>? bookings = null,
        List<Payment>? payments = null,
        List<BookingGuest>? bookingGuests = null,
        List<User>? users = null)
    {
        var ctx = Create();

        if (users is not null) ctx.Users.AddRange(users);
        if (rooms is not null) ctx.Rooms.AddRange(rooms);
        if (guests is not null) ctx.Guests.AddRange(guests);
        if (bookings is not null) ctx.Bookings.AddRange(bookings);
        if (payments is not null) ctx.Payments.AddRange(payments);
        if (bookingGuests is not null) ctx.BookingGuests.AddRange(bookingGuests);

        ctx.SaveChangesAsync().GetAwaiter().GetResult();
        return ctx;
    }

    private class TestDbContext : DbContext, IApplicationDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Guest> Guests => Set<Guest>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingGuest> BookingGuests => Set<BookingGuest>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(Infrastructure.Persistence.ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
