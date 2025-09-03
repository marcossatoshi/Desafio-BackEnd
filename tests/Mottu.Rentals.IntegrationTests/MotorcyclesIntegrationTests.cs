using System.Net.Http.Json;
using FluentAssertions;
using Mottu.Rentals.Contracts.Motorcycles;

namespace Mottu.Rentals.IntegrationTests;

public class MotorcyclesIntegrationTests : IClassFixture<ContainersFixture>
{
    private readonly ContainersFixture _fx;
    public MotorcyclesIntegrationTests(ContainersFixture fx) => _fx = fx;

    [Fact]
    public async Task Create_and_Get_Motorcycle_by_identifier()
    {
        var unique = Guid.NewGuid().ToString("N").Substring(0, 8);
        var create = new MotorcycleCreateRequest(
            Identifier: $"it-mc-{unique}",
            Year: 2024,
            Model: "IT Model",
            Plate: $"I{unique.Substring(0,2).ToUpper()}A{unique.Substring(2,3)}");

        var post = await _fx.Client.PostAsJsonAsync("/motorcycles", create);
        post.EnsureSuccessStatusCode();
        var created = await post.Content.ReadFromJsonAsync<MotorcycleResponse>();
        created.Should().NotBeNull();
        created!.Identifier.Should().Be(create.Identifier);

        var get = await _fx.Client.GetAsync($"/motorcycles/{created.Identifier}");
        get.EnsureSuccessStatusCode();
        var fetched = await get.Content.ReadFromJsonAsync<MotorcycleResponse>();
        fetched.Should().NotBeNull();
        fetched!.Identifier.Should().Be(create.Identifier);
        fetched!.Plate.Should().Be(create.Plate);
    }

    [Fact]
    public async Task List_Filter_By_Substring_Works()
    {
        var rnd = Guid.NewGuid().ToString("N").Substring(0,4).ToUpper();
        var create1 = await _fx.Client.PostAsJsonAsync("/motorcycles", new Mottu.Rentals.Contracts.Motorcycles.MotorcycleCreateRequest($"mc-int-3-{rnd}", 2024, "ModelA", $"ZZ{rnd}34"));
        create1.EnsureSuccessStatusCode();
        var create2 = await _fx.Client.PostAsJsonAsync("/motorcycles", new Mottu.Rentals.Contracts.Motorcycles.MotorcycleCreateRequest($"mc-int-4-{rnd}", 2023, "ModelB", $"12{rnd}Z4"));
        create2.EnsureSuccessStatusCode();

        var list = await _fx.Client.GetFromJsonAsync<List<Mottu.Rentals.Contracts.Motorcycles.MotorcycleResponse>>("/motorcycles?plate=12");
        Assert.NotNull(list);
        Assert.True(list!.Any(m => m.Plate.Contains("12")));
    }
}


