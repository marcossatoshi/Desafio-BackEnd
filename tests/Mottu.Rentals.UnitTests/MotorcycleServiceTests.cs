using FluentAssertions;
using Mottu.Rentals.Application.Abstractions;
using Mottu.Rentals.Application.Motorcycles;
using Mottu.Rentals.Contracts.Motorcycles;
using NSubstitute;

namespace Mottu.Rentals.UnitTests;

public class MotorcycleServiceTests
{
    [Fact]
    public async Task Create_ShouldConflict_WhenPlateExists()
    {
        var repo = Substitute.For<IMotorcycleRepository>();
        var publisher = Substitute.For<Mottu.Rentals.Application.Abstractions.IEventPublisher>();
        repo.ExistsPlateAsync("ABC1234", Arg.Any<CancellationToken>()).Returns(true);
        var sut = new MotorcycleService(repo, publisher);

        var act = async () => await sut.CreateAsync(new MotorcycleCreateRequest("mc-1", 2024, "ModelX", "ABC1234"), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}



