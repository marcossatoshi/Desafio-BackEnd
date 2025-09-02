using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mottu.Rentals.Contracts.Motorcycles;

namespace Mottu.Rentals.FunctionalTests;

public class MotorcyclesEndpointsTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    public MotorcyclesEndpointsTests(CustomWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Then_Get_List_ShouldReturnCreatedItem()
    {
        var resp = await _client.PostAsJsonAsync("/motorcycles", new MotorcycleCreateRequest("mc-func-1", 2024, "ModelZ", "XYZ1234"));
        if (resp.StatusCode != HttpStatusCode.Created)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Create failed: {(int)resp.StatusCode} {resp.StatusCode} Body: {body}");
        }
        var created = await resp.Content.ReadFromJsonAsync<MotorcycleResponse>();

        var list = await _client.GetFromJsonAsync<List<MotorcycleResponse>>("/motorcycles?plate=XYZ1234");
        list.Should().NotBeNull();
        list!.Any(x => x.Id == created!.Id).Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePlate_And_Delete_ShouldSucceed()
    {
        var create = await _client.PostAsJsonAsync("/motorcycles", new MotorcycleCreateRequest("mc-func-2", 2023, "ModelY", "AAA0001"));
        create.EnsureSuccessStatusCode();
        var created2 = await create.Content.ReadFromJsonAsync<MotorcycleResponse>();

        var upd = await _client.PutAsJsonAsync($"/motorcycles/{created2!.Id}/plate", new MotorcycleUpdatePlateRequest("BBB0002"));
        upd.EnsureSuccessStatusCode();

        var list2 = await _client.GetFromJsonAsync<List<MotorcycleResponse>>("/motorcycles?plate=BBB0002");
        list2!.Any(x => x.Id == created2!.Id && x.Plate == "BBB0002").Should().BeTrue();

        var del = await _client.DeleteAsync($"/motorcycles/{created2!.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}


