using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManager.Infrastructure.Persistence.Configurations;

public class BookingGuestConfiguration : IEntityTypeConfiguration<BookingGuest>
{
    public void Configure(EntityTypeBuilder<BookingGuest> builder)
    {
        builder.ToTable("BookingGuests");

        builder.HasKey(bg => new { bg.BookingId, bg.GuestId });

        builder.Property(bg => bg.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(bg => bg.Booking)
            .WithMany(b => b.BookingGuests)
            .HasForeignKey(bg => bg.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bg => bg.Guest)
            .WithMany(g => g.BookingGuests)
            .HasForeignKey(bg => bg.GuestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
