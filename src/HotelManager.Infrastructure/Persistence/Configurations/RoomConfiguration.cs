using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManager.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Number)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(r => r.Number).IsUnique();

        builder.Property(r => r.Floor).IsRequired();

        builder.Property(r => r.BedCount).IsRequired();

        builder.Property(r => r.BathroomType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.BasePricePerNight)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        builder.Property(r => r.IsUnderMaintenance)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
