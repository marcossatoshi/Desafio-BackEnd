using FluentAssertions;
using Mottu.Rentals.Application.Abstractions;
using Mottu.Rentals.Application.Rentals;
using Mottu.Rentals.Contracts.Rentals;
using Mottu.Rentals.Domain.Entities;
using Mottu.Rentals.Domain.Enums;
using NSubstitute;

namespace Mottu.Rentals.UnitTests;

public class RentalServiceTests
{
    private readonly IRentalRepository _rentalRepo = Substitute.For<IRentalRepository>();
    private readonly IMotorcycleRepository _motoRepo = Substitute.For<IMotorcycleRepository>();
    private readonly ICourierRepository _courierRepo = Substitute.For<ICourierRepository>();

    private RentalService CreateSut() => new(_rentalRepo, _motoRepo, _courierRepo, new DefaultRentalPricingStrategy());

    [Fact]
    public async Task Return_OnTime_ComputesTotal_AsPlanDaysTimesDaily()
    {
        var courier = new Courier { Id = Guid.NewGuid(), CnhType = CnhType.A };
        _courierRepo.GetByIdAsync(courier.Id, Arg.Any<CancellationToken>()).Returns(courier);
        _rentalRepo.ExistsActiveRentalForMotorcycleAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        _rentalRepo.ExistsActiveRentalForCourierAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var create = new RentalCreateRequest("rent-ontime", Guid.NewGuid(), courier.Id, (int)PlanType.Days7);
        var sut = CreateSut();
        var created = await sut.CreateAsync(create, CancellationToken.None);

        var entity = new Rental
        {
            Id = created.Id,
            CourierId = created.CourierId,
            MotorcycleId = created.MotorcycleId,
            Plan = PlanType.Days7,
            StartDate = created.StartDate,
            ExpectedEndDate = created.ExpectedEndDate,
            DailyPrice = 30m
        };
        _rentalRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var ret = await sut.ReturnAsync(entity.Id, new RentalReturnRequest(created.ExpectedEndDate), CancellationToken.None);
        ret!.TotalPrice.Should().Be(7 * 30m);
    }

    [Fact]
    public async Task Return_Early_AppliesPenalty_PerRules()
    {
        var courier = new Courier { Id = Guid.NewGuid(), CnhType = CnhType.A };
        _courierRepo.GetByIdAsync(courier.Id, Arg.Any<CancellationToken>()).Returns(courier);
        _rentalRepo.ExistsActiveRentalForMotorcycleAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        _rentalRepo.ExistsActiveRentalForCourierAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var create = new RentalCreateRequest("rent-early", Guid.NewGuid(), courier.Id, (int)PlanType.Days7);
        var sut = CreateSut();
        var created = await sut.CreateAsync(create, CancellationToken.None);

        var entity = new Rental
        {
            Id = created.Id,
            CourierId = created.CourierId,
            MotorcycleId = created.MotorcycleId,
            Plan = PlanType.Days7,
            StartDate = created.StartDate,
            ExpectedEndDate = created.ExpectedEndDate,
            DailyPrice = 30m
        };
        _rentalRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var endEarly = created.ExpectedEndDate.AddDays(-2);
        var ret = await sut.ReturnAsync(entity.Id, new RentalReturnRequest(endEarly), CancellationToken.None);

        var usedDays = endEarly.DayNumber - created.StartDate.DayNumber;
        var remainingDays = created.ExpectedEndDate.DayNumber - endEarly.DayNumber;
        var expected = (usedDays * 30m) + (remainingDays * 30m * 0.20m);
        ret!.TotalPrice.Should().Be(expected);
    }

    [Fact]
    public async Task Return_Late_ChargesExtraPerDay()
    {
        var courier = new Courier { Id = Guid.NewGuid(), CnhType = CnhType.A };
        _courierRepo.GetByIdAsync(courier.Id, Arg.Any<CancellationToken>()).Returns(courier);
        _rentalRepo.ExistsActiveRentalForMotorcycleAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        _rentalRepo.ExistsActiveRentalForCourierAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var create = new RentalCreateRequest("rent-late", Guid.NewGuid(), courier.Id, (int)PlanType.Days15);
        var sut = CreateSut();
        var created = await sut.CreateAsync(create, CancellationToken.None);

        var entity = new Rental
        {
            Id = created.Id,
            CourierId = created.CourierId,
            MotorcycleId = created.MotorcycleId,
            Plan = PlanType.Days15,
            StartDate = created.StartDate,
            ExpectedEndDate = created.ExpectedEndDate,
            DailyPrice = 28m
        };
        _rentalRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var endLate = created.ExpectedEndDate.AddDays(3);
        var ret = await sut.ReturnAsync(entity.Id, new RentalReturnRequest(endLate), CancellationToken.None);

        var expected = (15 * 28m) + (3 * 50m);
        ret!.TotalPrice.Should().Be(expected);
    }

    [Fact]
    public async Task Create_With_EndDate_Persists_EndDate()
    {
        var courier = new Courier { Id = Guid.NewGuid(), CnhType = CnhType.A };
        _courierRepo.GetByIdAsync(courier.Id, Arg.Any<CancellationToken>()).Returns(courier);
        _rentalRepo.ExistsActiveRentalForMotorcycleAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        _rentalRepo.ExistsActiveRentalForCourierAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var start = DateTime.UtcNow.Date.AddDays(-7);
        var expectedEnd = DateTime.UtcNow.Date.AddDays(-1);
        var end = expectedEnd;

        var req = new RentalCreateRequest("rent-with-end", Guid.NewGuid(), courier.Id, (int)PlanType.Days7, start, expectedEnd, end);
        var sut = CreateSut();
        var created = await sut.CreateAsync(req, CancellationToken.None);

        created.EndDate.Should().NotBeNull();
        created.EndDate!.Value.Should().Be(DateOnly.FromDateTime(end));
    }

    [Fact]
    public async Task Create_Without_EndDate_Leaves_EndDate_Null()
    {
        var courier = new Courier { Id = Guid.NewGuid(), CnhType = CnhType.A };
        _courierRepo.GetByIdAsync(courier.Id, Arg.Any<CancellationToken>()).Returns(courier);
        _rentalRepo.ExistsActiveRentalForMotorcycleAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        _rentalRepo.ExistsActiveRentalForCourierAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var req = new RentalCreateRequest("rent-no-end", Guid.NewGuid(), courier.Id, (int)PlanType.Days7);
        var sut = CreateSut();
        var created = await sut.CreateAsync(req, CancellationToken.None);

        created.EndDate.Should().BeNull();
    }
}



