using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mottu.Rentals.Contracts.Motorcycles;
using Mottu.Rentals.Contracts.Rentals;

namespace Mottu.Rentals.FunctionalTests;

public class MotorcyclesDeleteRulesTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    public MotorcyclesDeleteRulesTests(CustomWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Delete_should_conflict_when_active_rental_exists()
    {
        // Create motorcycle
        var mc = await _client.PostAsJsonAsync("/motorcycles", new MotorcycleCreateRequest("mc-del-1", 2024, "M1", "AAA1B23"));
        mc.EnsureSuccessStatusCode();
        var moto = await mc.Content.ReadFromJsonAsync<MotorcycleResponse>();

        // Create courier
        var co = await _client.PostAsJsonAsync("/couriers", new Mottu.Rentals.Contracts.Couriers.CourierCreateRequest(
            "co-del-1", "John Doe", "11222333000181", DateTime.UtcNow.AddYears(-25), "CNH123", "A"));
        co.EnsureSuccessStatusCode();
        var courier = await co.Content.ReadFromJsonAsync<Mottu.Rentals.Contracts.Couriers.CourierResponse>();

        // Create rental with no end date (active)
        var rent = await _client.PostAsJsonAsync("/rentals", new RentalCreateRequest(moto!.Id, courier!.Id, 7));
        rent.EnsureSuccessStatusCode();

        // Try delete motorcycle: conflicts
        var del = await _client.DeleteAsync($"/motorcycles/{moto!.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_should_succeed_when_rental_finished()
    {
        // Create motorcycle
        var mc = await _client.PostAsJsonAsync("/motorcycles", new MotorcycleCreateRequest("mc-del-2", 2024, "M2", "BBB2C34"));
        mc.EnsureSuccessStatusCode();
        var moto = await mc.Content.ReadFromJsonAsync<MotorcycleResponse>();

        // Create courier
        var co = await _client.PostAsJsonAsync("/couriers", new Mottu.Rentals.Contracts.Couriers.CourierCreateRequest(
            "co-del-2", "Jane Roe", "22333444000192", DateTime.UtcNow.AddYears(-30), "CNH456", "A"));
        co.EnsureSuccessStatusCode();
        var courier = await co.Content.ReadFromJsonAsync<Mottu.Rentals.Contracts.Couriers.CourierResponse>();

        // Create rental that already ended yesterday
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var rent = await _client.PostAsJsonAsync("/rentals", new RentalCreateRequest(
            moto!.Id, courier!.Id, 7));
        rent.EnsureSuccessStatusCode();

        // Delete motorcycle: should succeed
        var del = await _client.DeleteAsync($"/motorcycles/{moto!.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}


