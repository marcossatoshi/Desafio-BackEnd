using Microsoft.Extensions.Logging;
using Mottu.Rentals.Contracts.Rentals;

namespace Mottu.Rentals.Application.Rentals;

public sealed class LoggingRentalServiceDecorator : IRentalService
{
    private readonly IRentalService _inner;
    private readonly ILogger<LoggingRentalServiceDecorator> _logger;

    public LoggingRentalServiceDecorator(IRentalService inner, ILogger<LoggingRentalServiceDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<RentalResponse> CreateAsync(RentalCreateRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating rental for motorcycle {Motorcycle} and courier {Courier} plan {Plan}", request.MotorcycleId, request.CourierId, request.Plan);
        try
        {
            var res = await _inner.CreateAsync(request, ct);
            _logger.LogInformation("Created rental {Id}", res.Id);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create rental");
            throw;
        }
    }

    public async Task<RentalResponse?> ReturnAsync(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Returning rental {Id}", id);
        try
        {
            var res = await _inner.ReturnAsync(id, ct);
            if (res is null)
            {
                _logger.LogWarning("Rental {Id} not found on return", id);
            }
            else
            {
                _logger.LogInformation("Returned rental {Id} total {Total}", res.Id, res.TotalPrice);
            }
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to return rental {Id}", id);
            throw;
        }
    }
}


