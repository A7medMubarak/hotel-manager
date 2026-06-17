using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Domain.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Guest> Guests { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<BookingGuest> BookingGuests { get; }
    DbSet<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
