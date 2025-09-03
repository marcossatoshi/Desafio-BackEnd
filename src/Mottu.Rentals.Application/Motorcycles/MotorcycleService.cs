using Mottu.Rentals.Contracts.Motorcycles;
using Mottu.Rentals.Domain.Entities;
using Mottu.Rentals.Application.Abstractions;
using System.Text.Json;

namespace Mottu.Rentals.Application.Motorcycles;

public class MotorcycleService : IMotorcycleService
{
    private readonly IMotorcycleRepository _repo;
    private readonly IEventPublisher _publisher;

    public MotorcycleService(IMotorcycleRepository repo, IEventPublisher publisher)
    {
        _repo = repo;
        _publisher = publisher;
    }

    public async Task<MotorcycleResponse> CreateAsync(MotorcycleCreateRequest request, CancellationToken ct)
    {
        var existsPlate = await _repo.ExistsPlateAsync(request.Plate, ct);
        if (existsPlate)
        {
            throw new InvalidOperationException("Plate already exists.");
        }

        var entity = new Motorcycle
        {
            Id = Guid.NewGuid(),
            Identifier = request.Identifier ?? string.Empty,
            Year = request.Year,
            Model = request.Model,
            Plate = request.Plate,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        var evt = new Mottu.Rentals.Contracts.Events.MotorcycleCreatedEvent(entity.Id, entity.Year, entity.Model, entity.Plate, entity.CreatedAtUtc);
        try
        {
            await _publisher.PublishAsync(evt, "motorcycle.created", ct);
        }
        catch (Exception)
        {
            // Messaging is best-effort; do not fail the API operation if broker is unavailable
        }

        return new MotorcycleResponse(entity.Id, entity.Identifier, entity.Year, entity.Model, entity.Plate, entity.CreatedAtUtc);
    }

    public async Task<IReadOnlyList<MotorcycleResponse>> ListAsync(string? plate, CancellationToken ct)
    {
        var items = await _repo.ListAsync(plate, ct);
        return items
            .Select(x => new MotorcycleResponse(x.Id, x.Identifier, x.Year, x.Model, x.Plate, x.CreatedAtUtc))
            .ToList();
    }

    public async Task<MotorcycleResponse?> UpdatePlateAsync(Guid id, MotorcycleUpdatePlateRequest request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return null;

        var plateInUse = await _repo.PlateInUseByOtherAsync(id, request.Plate, ct);
        if (plateInUse)
        {
            throw new InvalidOperationException("Plate already exists.");
        }

        entity.Plate = request.Plate;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _repo.SaveChangesAsync(ct);
        return new MotorcycleResponse(entity.Id, entity.Identifier, entity.Year, entity.Model, entity.Plate, entity.CreatedAtUtc);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var hasRental = await _repo.HasRentalAsync(id, ct);
        if (hasRental) return false;

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return false;

        await _repo.RemoveAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}


