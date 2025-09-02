using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Mottu.Rentals.Contracts.Motorcycles;
using Mottu.Rentals.Infrastructure.Persistence;

namespace Mottu.Rentals.IntegrationTests;

public class MotorcycleNotificationIntegrationTests : IClassFixture<ContainersFixture>
{
    private readonly ContainersFixture _fx;
    public MotorcycleNotificationIntegrationTests(ContainersFixture fx) => _fx = fx;

    [Fact]
    public async Task Creating_2024_motorcycle_persists_notification()
    {
        var unique = Guid.NewGuid().ToString("N").Substring(0, 8);
        var create = new MotorcycleCreateRequest(
            Identifier: $"it-mc-{unique}",
            Year: 2024,
            Model: "IT 2024",
            Plate: $"I{unique.Substring(0,2).ToUpper()}B{unique.Substring(2,3)}");

        var post = await _fx.Client.PostAsJsonAsync("/motorcycles", create);
        post.EnsureSuccessStatusCode();

        // poll up to ~5s for async processing
        var attempts = 0;
        int count = 0;
        do
        {
            using var scope = _fx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RentalsDbContext>();
            count = await db.MotorcycleCreatedNotifications.CountAsync();
            if (count > 0) break;
            await Task.Delay(250);
        } while (++attempts < 20);

        count.Should().BeGreaterThan(0);
    }
}


