using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mottu.Rentals.Infrastructure.Entities;

namespace Mottu.Rentals.Infrastructure.Persistence.Configurations;

public class MotorcycleCreatedNotificationConfiguration : IEntityTypeConfiguration<MotorcycleCreatedNotification>
{
    public void Configure(EntityTypeBuilder<MotorcycleCreatedNotification> builder)
    {
        builder.ToTable("motorcycle_created_notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.MotorcycleId).HasColumnName("motorcycle_id");
        builder.Property(x => x.Year).HasColumnName("year");
        builder.Property(x => x.PublishedAtUtc).HasColumnName("published_at_utc");
    }
}


