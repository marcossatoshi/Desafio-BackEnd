using Microsoft.EntityFrameworkCore;
using Mottu.Rentals.Domain.Entities;
using Mottu.Rentals.Infrastructure.Entities;

namespace Mottu.Rentals.Infrastructure.Persistence;

public class RentalsDbContext : DbContext
{
    public RentalsDbContext(DbContextOptions<RentalsDbContext> options) : base(options)
    {
    }

    public DbSet<Motorcycle> Motorcycles => Set<Motorcycle>();
    public DbSet<Courier> Couriers => Set<Courier>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<MotorcycleCreatedNotification> MotorcycleCreatedNotifications => Set<MotorcycleCreatedNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RentalsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}


