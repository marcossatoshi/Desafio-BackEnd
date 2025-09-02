using Mottu.Rentals.Contracts.Rentals;

namespace Mottu.Rentals.Application.Rentals;

public interface IRentalService
{
    Task<RentalResponse> CreateAsync(RentalCreateRequest request, CancellationToken ct);
    Task<RentalResponse?> ReturnAsync(Guid id, RentalReturnRequest request, CancellationToken ct);
}


