using Mottu.Rentals.Domain.Entities;

namespace Mottu.Rentals.Application.Abstractions;

public interface ICourierRepository
{
    Task<bool> ExistsCnpjAsync(string cnpj, CancellationToken ct);
    Task<bool> ExistsCnhNumberAsync(string cnhNumber, CancellationToken ct);
    Task<bool> ExistsIdentifierAsync(string identifier, CancellationToken ct);
    Task AddAsync(Courier entity, CancellationToken ct);
    Task<Courier?> GetByIdAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}


