using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mottu.Rentals.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mottu.Rentals.FunctionalTests;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DbRoot = new();
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var dbName = $"tests-db-{Guid.NewGuid()}";
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UseMassTransitInMemory"] = "true",
                ["UseInMemoryEF"] = "true"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RentalsDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            services.AddDbContext<RentalsDbContext>(options => options.UseInMemoryDatabase(dbName, DbRoot));
            services.AddSingleton<Mottu.Rentals.Application.Abstractions.IEventPublisher, NoopPublisher>();
            // no explicit EnsureCreated here to avoid provider conflicts
        });
        return base.CreateHost(builder);
    }
}

public class NoopPublisher : Mottu.Rentals.Application.Abstractions.IEventPublisher
{
    public Task PublishAsync<T>(T message, string routingKey = "", CancellationToken cancellationToken = default) => Task.CompletedTask;
}


