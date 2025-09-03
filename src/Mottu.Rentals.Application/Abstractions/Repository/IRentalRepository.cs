using Mottu.Rentals.Domain.Entities;

namespace Mottu.Rentals.Application.Abstractions;

public interface IRentalRepository
{
    Task AddAsync(Rental entity, CancellationToken ct);
    Task<Rental?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsActiveRentalForMotorcycleAsync(Guid motorcycleId, CancellationToken ct);
    Task<bool> ExistsActiveRentalForCourierAsync(Guid courierId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}


