using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mottu.Rentals.Domain.Entities;

namespace Mottu.Rentals.Infrastructure.Persistence.Configurations;

public class MotorcycleConfiguration : IEntityTypeConfiguration<Motorcycle>
{
    public void Configure(EntityTypeBuilder<Motorcycle> builder)
    {
        builder.ToTable("motorcycles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Identifier).HasColumnName("identifier").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Year).HasColumnName("year").IsRequired();
        builder.Property(x => x.Model).HasColumnName("model").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Plate).HasColumnName("plate").HasMaxLength(10).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(x => x.Plate).IsUnique();
        builder.HasIndex(x => x.Identifier);
    }
}


