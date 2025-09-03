using Mottu.Rentals.Application.Abstractions;
using Mottu.Rentals.Contracts.Rentals;
using Mottu.Rentals.Domain.Entities;
using Mottu.Rentals.Domain.Enums;

namespace Mottu.Rentals.Application.Rentals;

public class RentalService : IRentalService
{
    private readonly IRentalRepository _repo;
    private readonly IMotorcycleRepository _motoRepo;
    private readonly ICourierRepository _courierRepo;
    private readonly IRentalPricingStrategy _pricing;

    public RentalService(IRentalRepository repo, IMotorcycleRepository motoRepo, ICourierRepository courierRepo, IRentalPricingStrategy pricing)
    {
        _repo = repo;
        _motoRepo = motoRepo;
        _courierRepo = courierRepo;
        _pricing = pricing;
    }

    public async Task<RentalResponse> CreateAsync(RentalCreateRequest request, CancellationToken ct)
    {
        var plan = (PlanType)request.Plan;
        if (!Enum.IsDefined(typeof(PlanType), plan))
            throw new InvalidOperationException("Invalid plan.");

        var courier = await _courierRepo.GetByIdAsync(request.CourierId, ct) ?? throw new InvalidOperationException("Courier not found.");
        if (courier.CnhType != CnhType.A && courier.CnhType != CnhType.AB)
            throw new InvalidOperationException("Courier not allowed.");

        if (await _repo.ExistsActiveRentalForMotorcycleAsync(request.MotorcycleId, ct))
            throw new InvalidOperationException("Motorcycle already rented.");
        if (await _repo.ExistsActiveRentalForCourierAsync(request.CourierId, ct))
            throw new InvalidOperationException("Courier already has active rental.");

        var start = Mottu.Rentals.Application.Common.Time.BrazilTime.Today().AddDays(1);
        var expected = start.AddDays((int)plan);
        var daily = _pricing.DetermineDailyPrice(plan);

        var entity = new Rental
        {
            Id = Guid.NewGuid(),
            MotorcycleId = request.MotorcycleId,
            CourierId = request.CourierId,
            Plan = plan,
            StartDate = start,
            ExpectedEndDate = expected,
            DailyPrice = daily,
            CreatedAtUtc = Mottu.Rentals.Application.Common.Time.BrazilTime.Now()
        };
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return ToResponse(entity);
    }

    public async Task<RentalResponse?> ReturnAsync(Guid id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return null;
        var end = Mottu.Rentals.Application.Common.Time.BrazilTime.Today();
        entity.EndDate = end;

        var total = _pricing.CalculateTotalOnReturn(entity, end);

        entity.TotalPrice = decimal.Round(total, 2, MidpointRounding.AwayFromZero);
        await _repo.SaveChangesAsync(ct);
        return ToResponse(entity);
    }

    private static RentalResponse ToResponse(Rental e) => new(
        e.Id, e.MotorcycleId, e.CourierId, (int)e.Plan, e.StartDate, e.ExpectedEndDate, e.EndDate, e.DailyPrice, e.TotalPrice
    );
}


