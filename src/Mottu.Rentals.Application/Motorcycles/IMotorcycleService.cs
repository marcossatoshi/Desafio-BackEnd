using Mottu.Rentals.Contracts.Motorcycles;

namespace Mottu.Rentals.Application.Motorcycles;

public interface IMotorcycleService
{
    Task<MotorcycleResponse> CreateAsync(MotorcycleCreateRequest request, CancellationToken ct);
    Task<IReadOnlyList<MotorcycleResponse>> ListAsync(string? plate, CancellationToken ct);
    Task<MotorcycleResponse?> UpdatePlateAsync(Guid id, MotorcycleUpdatePlateRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}


