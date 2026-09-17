using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManager.Infrastructure.Persistence.Configurations;

public class BookingNightConfiguration : IEntityTypeConfiguration<BookingNight>
{
    public void Configure(EntityTypeBuilder<BookingNight> builder)
    {
        builder.ToTable("BookingNights");
        builder.HasKey(bn => bn.Id);
        builder.Property(bn => bn.Date).IsRequired();
        builder.HasOne(bn => bn.Booking)
            .WithMany(b => b.BookingNights)
            .HasForeignKey(bn => bn.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(bn => bn.Room)
            .WithMany()
            .HasForeignKey(bn => bn.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(bn => new { bn.RoomId, bn.Date }).IsUnique();
    }
}
