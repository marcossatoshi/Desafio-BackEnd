using Mottu.Rentals.Domain.Entities;

namespace Mottu.Rentals.Application.Abstractions;

public interface IMotorcycleRepository
{
    Task<bool> ExistsPlateAsync(string plate, CancellationToken ct);
    Task AddAsync(Motorcycle entity, CancellationToken ct);
    Task<List<Motorcycle>> ListAsync(string? plate, CancellationToken ct);
    Task<Motorcycle?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> PlateInUseByOtherAsync(Guid id, string plate, CancellationToken ct);
    Task<bool> HasRentalAsync(Guid motorcycleId, CancellationToken ct);
    Task RemoveAsync(Motorcycle entity, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}


