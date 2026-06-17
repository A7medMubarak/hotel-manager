using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManager.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.PaymentDate).IsRequired();

        builder.Property(p => p.Notes).HasMaxLength(300);

        builder.Property(p => p.CreatedByUserId).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasOne(p => p.Booking)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
