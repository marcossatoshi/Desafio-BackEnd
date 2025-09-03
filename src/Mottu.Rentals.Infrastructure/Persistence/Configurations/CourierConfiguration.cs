using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mottu.Rentals.Domain.Entities;

namespace Mottu.Rentals.Infrastructure.Persistence.Configurations;

public class CourierConfiguration : IEntityTypeConfiguration<Courier>
{
    public void Configure(EntityTypeBuilder<Courier> builder)
    {
        builder.ToTable("couriers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Identifier).HasColumnName("identifier").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Cnpj).HasColumnName("cnpj").HasMaxLength(20).IsRequired();
        builder.Property(x => x.BirthDate).HasColumnName("birth_date").IsRequired();
        builder.Property(x => x.CnhNumber).HasColumnName("cnh_number").HasMaxLength(30).IsRequired();
        builder.Property(x => x.CnhType).HasColumnName("cnh_type").HasConversion<int>().IsRequired();
        builder.Property(x => x.CnhImagePath).HasColumnName("cnh_image_path");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(x => x.Cnpj).IsUnique();
        builder.HasIndex(x => x.CnhNumber).IsUnique();
        builder.HasIndex(x => x.Identifier).IsUnique();
    }
}


