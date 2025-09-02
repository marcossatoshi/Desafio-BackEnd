using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mottu.Rentals.Domain.Entities;

namespace Mottu.Rentals.Infrastructure.Persistence.Configurations;

public class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        builder.ToTable("rentals");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Identifier).HasColumnName("identifier").HasMaxLength(100).IsRequired();
        builder.Property(x => x.MotorcycleId).HasColumnName("motorcycle_id").IsRequired();
        builder.Property(x => x.CourierId).HasColumnName("courier_id").IsRequired();
        builder.Property(x => x.Plan).HasColumnName("plan").HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.StartDate).HasColumnName("start_date").IsRequired();
        builder.Property(x => x.ExpectedEndDate).HasColumnName("expected_end_date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnName("end_date");
        builder.Property(x => x.DailyPrice).HasColumnName("daily_price").HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(x => x.TotalPrice).HasColumnName("total_price").HasColumnType("numeric(10,2)");
        builder.HasIndex(x => x.Identifier);

        builder.HasOne<Motorcycle>()
            .WithMany()
            .HasForeignKey(x => x.MotorcycleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Courier>()
            .WithMany()
            .HasForeignKey(x => x.CourierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


