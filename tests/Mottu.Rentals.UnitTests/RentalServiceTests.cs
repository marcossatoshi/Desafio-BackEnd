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

        var create = new RentalCreateRequest(Guid.NewGuid(), courier.Id, (int)PlanType.Days7);
        var sut = CreateSut();
        var created = await sut.CreateAsync(create, CancellationToken.None);

        // Make today be the expected end date for on-time scenario
        var today = Mottu.Rentals.Application.Common.Time.BrazilTime.Today();
        var entity = new Rental
        {
            Id = created.Id,
            CourierId = created.CourierId,
            MotorcycleId = created.MotorcycleId,
            Plan = PlanType.Days7,
            StartDate = today.AddDays(-7),
            ExpectedEndDate = today,
            DailyPrice = 30m
        };
        _rentalRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var ret = await sut.ReturnAsync(entity.Id, CancellationToken.None);
        ret!.TotalPrice.Should().Be(7 * 30m);
    }

    [Fact]
    public async Task Return_Early_AppliesPenalty_PerRules()
    {
        var courier = new Courier { Id = Guid.NewGuid(), CnhType = CnhType.A };
        _courierRepo.GetByIdAsync(courier.Id, Arg.Any<CancellationToken>()).Returns(courier);
        _rentalRepo.ExistsActiveRentalForMotorcycleAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        _rentalRepo.ExistsActiveRentalForCourierAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var create = new RentalCreateRequest(Guid.NewGuid(), courier.Id, (int)PlanType.Days7);
        var sut = CreateSut();
        var created = await sut.CreateAsync(create, CancellationToken.None);

        // Early return: today is 2 days before expected end
        var today = Mottu.Rentals.Application.Common.Time.BrazilTime.Today();
        var entity = new Rental
        {
            Id = created.Id,
            CourierId = created.CourierId,
            MotorcycleId = created.MotorcycleId,
            Plan = PlanType.Days7,
            StartDate = today.AddDays(-5),
            ExpectedEndDate = today.AddDays(2),
            DailyPrice = 30m
        };
        _rentalRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var ret = await sut.ReturnAsync(entity.Id, CancellationToken.None);

        var usedDays = 5; // from StartDate = today-5 until end(today)
        var remainingDays = 2; // expected end in 2 days
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

        var create = new RentalCreateRequest(Guid.NewGuid(), courier.Id, (int)PlanType.Days15);
        var sut = CreateSut();
        var created = await sut.CreateAsync(create, CancellationToken.None);

        // Late return: today is 3 days after expected end
        var today = Mottu.Rentals.Application.Common.Time.BrazilTime.Today();
        var entity = new Rental
        {
            Id = created.Id,
            CourierId = created.CourierId,
            MotorcycleId = created.MotorcycleId,
            Plan = PlanType.Days15,
            StartDate = today.AddDays(-18),
            ExpectedEndDate = today.AddDays(-3),
            DailyPrice = 28m
        };
        _rentalRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var ret = await sut.ReturnAsync(entity.Id, CancellationToken.None);

        var expected = (15 * 28m) + (3 * 50m);
        ret!.TotalPrice.Should().Be(expected);
    }

    [Fact]
    public async Task Create_Without_EndDate_Leaves_EndDate_Null()
    {
        var courier = new Courier { Id = Guid.NewGuid(), CnhType = CnhType.A };
        _courierRepo.GetByIdAsync(courier.Id, Arg.Any<CancellationToken>()).Returns(courier);
        _rentalRepo.ExistsActiveRentalForMotorcycleAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        _rentalRepo.ExistsActiveRentalForCourierAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var req = new RentalCreateRequest(Guid.NewGuid(), courier.Id, (int)PlanType.Days7);
        var sut = CreateSut();
        var created = await sut.CreateAsync(req, CancellationToken.None);

        created.EndDate.Should().BeNull();
    }

    [Fact]
    public async Task Return_Twice_Should_Throw_InvalidOperation()
    {
        var courier = new Courier { Id = Guid.NewGuid(), CnhType = CnhType.A };
        _courierRepo.GetByIdAsync(courier.Id, Arg.Any<CancellationToken>()).Returns(courier);
        _rentalRepo.ExistsActiveRentalForMotorcycleAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        _rentalRepo.ExistsActiveRentalForCourierAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var create = new RentalCreateRequest(Guid.NewGuid(), courier.Id, (int)PlanType.Days7);
        var sut = CreateSut();
        var created = await sut.CreateAsync(create, CancellationToken.None);

        var finalized = new Rental
        {
            Id = created.Id,
            CourierId = created.CourierId,
            MotorcycleId = created.MotorcycleId,
            Plan = PlanType.Days7,
            StartDate = created.StartDate,
            ExpectedEndDate = created.ExpectedEndDate,
            EndDate = created.ExpectedEndDate,
            DailyPrice = 30m
        };
        _rentalRepo.GetByIdAsync(finalized.Id, Arg.Any<CancellationToken>()).Returns(finalized);

        var act = () => sut.ReturnAsync(finalized.Id, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}



