using Mottu.Rentals.Contracts.Couriers;

namespace Mottu.Rentals.Application.Couriers;

public interface ICourierService
{
    Task<CourierResponse> CreateAsync(CourierCreateRequest request, CancellationToken ct);
    Task<CourierResponse?> UploadCnhAsync(Guid id, string fileName, string contentType, Stream content, CancellationToken ct);
}


