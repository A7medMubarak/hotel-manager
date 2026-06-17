using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManager.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.CheckIn).IsRequired();
        builder.Property(b => b.CheckOut).IsRequired();

        builder.Property(b => b.PricePerNight)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(b => b.Notes).HasMaxLength(500);

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.CreatedByUserId).IsRequired();

        builder.HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.CreatedBy)
            .WithMany()
            .HasForeignKey(b => b.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
