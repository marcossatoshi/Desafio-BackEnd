using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mottu.Rentals.Contracts.Couriers;
using Mottu.Rentals.Contracts.Motorcycles;
using Mottu.Rentals.Contracts.Rentals;

namespace Mottu.Rentals.FunctionalTests;

public class CouriersDeleteRulesTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    public CouriersDeleteRulesTests(CustomWebAppFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Delete_courier_conflicts_with_active_rental()
    {
        var mc = await _client.PostAsJsonAsync("/motorcycles", new MotorcycleCreateRequest("mc-co-1", 2024, "MX", "CCD3E45"));
        mc.EnsureSuccessStatusCode();
        var moto = await mc.Content.ReadFromJsonAsync<MotorcycleResponse>();

        var co = await _client.PostAsJsonAsync("/couriers", new CourierCreateRequest(
            "co-1", "John Active", "33444555000103", DateTime.UtcNow.AddYears(-22), "CNH900", "A"));
        co.EnsureSuccessStatusCode();
        var courier = await co.Content.ReadFromJsonAsync<CourierResponse>();

        var rent = await _client.PostAsJsonAsync("/rentals", new RentalCreateRequest(moto!.Id, courier!.Id, 7));
        rent.EnsureSuccessStatusCode();

        var del = await _client.DeleteAsync($"/couriers/{courier!.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_courier_succeeds_when_only_finished_rentals()
    {
        var mc = await _client.PostAsJsonAsync("/motorcycles", new MotorcycleCreateRequest("mc-co-2", 2024, "MY", "DDE4F56"));
        mc.EnsureSuccessStatusCode();
        var moto = await mc.Content.ReadFromJsonAsync<MotorcycleResponse>();

        var co = await _client.PostAsJsonAsync("/couriers", new CourierCreateRequest(
            "co-2", "Jane Finished", "44555666000114", DateTime.UtcNow.AddYears(-28), "CNH901", "A"));
        co.EnsureSuccessStatusCode();
        var courier = await co.Content.ReadFromJsonAsync<CourierResponse>();

        var rent = await _client.PostAsJsonAsync("/rentals", new RentalCreateRequest(moto!.Id, courier!.Id, 7));
        rent.EnsureSuccessStatusCode();
        var createdRent = await rent.Content.ReadFromJsonAsync<Mottu.Rentals.Contracts.Rentals.RentalResponse>();
        var retResp = await _client.PostAsync($"/rentals/{createdRent!.Id}/return", null);
        retResp.EnsureSuccessStatusCode();

        var del = await _client.DeleteAsync($"/couriers/{courier!.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}


