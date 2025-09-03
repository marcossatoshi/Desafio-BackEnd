using Microsoft.Extensions.Logging;
using Mottu.Rentals.Contracts.Couriers;

namespace Mottu.Rentals.Application.Couriers;

public sealed class LoggingCourierServiceDecorator : ICourierService
{
    private readonly ICourierService _inner;
    private readonly ILogger<LoggingCourierServiceDecorator> _logger;

    public LoggingCourierServiceDecorator(ICourierService inner, ILogger<LoggingCourierServiceDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<CourierResponse> CreateAsync(CourierCreateRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating courier {Name}", request.Name);
        try
        {
            var res = await _inner.CreateAsync(request, ct);
            _logger.LogInformation("Created courier {Id} ({Identifier})", res.Id, res.Identifier);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create courier");
            throw;
        }
    }

    public async Task<CourierResponse?> UploadCnhAsync(Guid id, string fileName, string contentType, Stream content, CancellationToken ct)
    {
        _logger.LogInformation("Uploading CNH for courier {Id} file {File}", id, fileName);
        try
        {
            var res = await _inner.UploadCnhAsync(id, fileName, contentType, content, ct);
            if (res is null)
                _logger.LogWarning("Courier {Id} not found for CNH upload", id);
            else
                _logger.LogInformation("Uploaded CNH for courier {Id}", id);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload CNH for courier {Id}", id);
            throw;
        }
    }
}


