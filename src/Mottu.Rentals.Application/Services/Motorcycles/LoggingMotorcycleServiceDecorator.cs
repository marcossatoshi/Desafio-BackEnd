using Microsoft.Extensions.Logging;
using Mottu.Rentals.Contracts.Motorcycles;

namespace Mottu.Rentals.Application.Motorcycles;

public sealed class LoggingMotorcycleServiceDecorator : IMotorcycleService
{
    private readonly IMotorcycleService _inner;
    private readonly ILogger<LoggingMotorcycleServiceDecorator> _logger;

    public LoggingMotorcycleServiceDecorator(IMotorcycleService inner, ILogger<LoggingMotorcycleServiceDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<MotorcycleResponse> CreateAsync(MotorcycleCreateRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating motorcycle {Model} {Year} plate {Plate}", request.Model, request.Year, request.Plate);
        try
        {
            var res = await _inner.CreateAsync(request, ct);
            _logger.LogInformation("Created motorcycle {Id} ({Identifier})", res.Id, res.Identifier);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create motorcycle with plate {Plate}", request.Plate);
            throw;
        }
    }

    public async Task<IReadOnlyList<MotorcycleResponse>> ListAsync(string? plate, CancellationToken ct)
    {
        _logger.LogDebug("Listing motorcycles, plate filter: {Plate}", plate);
        return await _inner.ListAsync(plate, ct);
    }

    public async Task<MotorcycleResponse?> UpdatePlateAsync(Guid id, MotorcycleUpdatePlateRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Updating motorcycle {Id} plate to {Plate}", id, request.Plate);
        try
        {
            var res = await _inner.UpdatePlateAsync(id, request, ct);
            if (res is null)
                _logger.LogWarning("Motorcycle {Id} not found for plate update", id);
            else
                _logger.LogInformation("Updated motorcycle {Id} plate to {Plate}", id, res.Plate);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update plate for motorcycle {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Deleting motorcycle {Id}", id);
        try
        {
            var ok = await _inner.DeleteAsync(id, ct);
            if (!ok)
                _logger.LogWarning("Motorcycle {Id} not found or has active rental", id);
            else
                _logger.LogInformation("Deleted motorcycle {Id}", id);
            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete motorcycle {Id}", id);
            throw;
        }
    }
}


