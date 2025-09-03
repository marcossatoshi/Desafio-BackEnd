using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Mottu.Rentals.Infrastructure.Persistence;

namespace Mottu.Rentals.IntegrationTests;

public sealed class ContainersFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private readonly RabbitMqContainer? _rabbitMq;
    public HttpClient Client { get; private set; } = default!;
    public WebApplicationFactory<Program> Factory { get; private set; } = default!;

    public ContainersFixture()
    {
        // Improve compatibility when running under local GitHub Actions (act): disable Ryuk
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");

        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithDatabase("mottu_rentals")
            .Build();

        var isAct = string.Equals(Environment.GetEnvironmentVariable("ACT"), "true", StringComparison.OrdinalIgnoreCase);
        if (!isAct)
        {
            _rabbitMq = new RabbitMqBuilder()
                .WithImage("rabbitmq:3-management")
                .WithUsername("guest")
                .WithPassword("guest")
                .Build();
        }
    }

    public async Task InitializeAsync()
    {
        var isAct = string.Equals(Environment.GetEnvironmentVariable("ACT"), "true", StringComparison.OrdinalIgnoreCase);
        if (!isAct)
        {
            await _postgres.StartAsync();
            await _rabbitMq!.StartAsync();
            var cs = _postgres.GetConnectionString();
            cs = cs.Replace("Host=localhost", "Host=127.0.0.1");
            var rmqHost = _rabbitMq!.Hostname;
            var rmqPort = _rabbitMq.GetMappedPublicPort(5672);
            Environment.SetEnvironmentVariable("UseInMemoryEF", null);
            Environment.SetEnvironmentVariable("RabbitMq__HostName", rmqHost);
            Environment.SetEnvironmentVariable("RabbitMq__Port", rmqPort.ToString());
            Environment.SetEnvironmentVariable("RabbitMq__UserName", "guest");
            Environment.SetEnvironmentVariable("RabbitMq__Password", "guest");
            Environment.SetEnvironmentVariable("UseMassTransitInMemory", null);

            // Build factory bound to this fixture's Postgres connection
            Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
            {
                b.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RentalsDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    services.AddDbContext<RentalsDbContext>(options => options.UseNpgsql(cs));
                });
            });
        }
        else
        {
            Environment.SetEnvironmentVariable("UseInMemoryEF", "true");
            Environment.SetEnvironmentVariable("UseMassTransitInMemory", "true");
            Factory = new WebApplicationFactory<Program>();
        }

        if (!isAct)
        {
            // Apply EF migrations once the factory is built, resetting database to avoid schema drift
            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<RentalsDbContext>();
                await db.Database.MigrateAsync();
            }
        }
        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        try { Client?.Dispose(); } catch { }
        try { Factory?.Dispose(); } catch { }
        try { if (_rabbitMq is not null) await _rabbitMq.DisposeAsync(); } catch { }
        try { await _postgres.DisposeAsync(); } catch { }
    }
}


