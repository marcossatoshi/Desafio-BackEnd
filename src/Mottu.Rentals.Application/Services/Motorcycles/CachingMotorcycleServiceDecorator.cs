using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Mottu.Rentals.Contracts.Motorcycles;

namespace Mottu.Rentals.Application.Motorcycles;

public sealed class CachingMotorcycleServiceDecorator : IMotorcycleService
{
    private readonly IMotorcycleService _inner;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingMotorcycleServiceDecorator> _logger;

    public CachingMotorcycleServiceDecorator(
        IMotorcycleService inner,
        IMemoryCache cache,
        ILogger<CachingMotorcycleServiceDecorator> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<MotorcycleResponse> CreateAsync(MotorcycleCreateRequest request, CancellationToken ct)
    {
        var result = await _inner.CreateAsync(request, ct);
        InvalidateListCache(request.Plate);
        return result;
    }

    public async Task<IReadOnlyList<MotorcycleResponse>> ListAsync(string? plate, CancellationToken ct)
    {
        var key = BuildListKey(plate);
        if (_cache.TryGetValue(key, out IReadOnlyList<MotorcycleResponse>? cached) && cached is not null)
        {
            _logger.LogDebug("Cache hit for {Key}", key);
            return cached;
        }

        var data = await _inner.ListAsync(plate, ct);
        _cache.Set(key, data, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15)
        });
        _logger.LogDebug("Cache set for {Key}", key);
        return data;
    }

    public async Task<MotorcycleResponse?> UpdatePlateAsync(Guid id, MotorcycleUpdatePlateRequest request, CancellationToken ct)
    {
        var result = await _inner.UpdatePlateAsync(id, request, ct);
        InvalidateListCache(request.Plate);
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var ok = await _inner.DeleteAsync(id, ct);
        if (ok) InvalidateListCache(null);
        return ok;
    }

    private static string BuildListKey(string? plate) => $"motorcycles:list:{(plate ?? "*")}";

    private void InvalidateListCache(string? affectedPlate)
    {
        _cache.Remove(BuildListKey(affectedPlate));
        _cache.Remove(BuildListKey(null));
    }
}


