using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mottu.Rentals.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RentalsDbContext>
{
    public RentalsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RentalsDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("MOTTU_POSTGRES_CONNECTION")
                               ?? "Host=localhost;Port=5432;Database=mottu_rentals;Username=postgres;Password=postgres";
        optionsBuilder.UseNpgsql(connectionString);
        return new RentalsDbContext(optionsBuilder.Options);
    }
}


