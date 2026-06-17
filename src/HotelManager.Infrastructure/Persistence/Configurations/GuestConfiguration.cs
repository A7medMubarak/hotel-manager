using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManager.Infrastructure.Persistence.Configurations;

public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        builder.ToTable("Guests");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.NationalId)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(g => g.NationalId).IsUnique();

        builder.Property(g => g.Address)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(g => g.Phone)
            .HasMaxLength(20);

        builder.Property(g => g.CreatedAt)
            .IsRequired();
    }
}
