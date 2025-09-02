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
}


