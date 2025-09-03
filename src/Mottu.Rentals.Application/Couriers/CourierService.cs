using Mottu.Rentals.Application.Abstractions;
using Mottu.Rentals.Contracts.Couriers;
using Mottu.Rentals.Domain.Entities;
using Mottu.Rentals.Domain.Enums;
using Mottu.Rentals.Application.Common.Validation;

namespace Mottu.Rentals.Application.Couriers;

public class CourierService : ICourierService
{
    private readonly ICourierRepository _repo;
    private readonly IFileStorage _storage;

    public CourierService(ICourierRepository repo, IFileStorage storage)
    {
        _repo = repo;
        _storage = storage;
    }

    public async Task<CourierResponse> CreateAsync(CourierCreateRequest request, CancellationToken ct)
    {
        if (await _repo.ExistsCnpjAsync(request.Cnpj, ct))
            throw new InvalidOperationException("CNPJ already exists.");
        if (await _repo.ExistsCnhNumberAsync(request.CnhNumber, ct))
            throw new InvalidOperationException("CNH number already exists.");

        var cnhType = request.CnhType?.ToUpperInvariant() switch
        {
            "A" => CnhType.A,
            "B" => CnhType.B,
            "A+B" or "AB" => CnhType.AB,
            _ => throw new InvalidOperationException("Invalid CNH type.")
        };

        var entity = new Courier
        {
            Id = Guid.NewGuid(),
            Identifier = request.Identifier ?? string.Empty,
            Name = request.Name,
            Cnpj = request.Cnpj,
            BirthDate = request.BirthDate,
            CnhNumber = request.CnhNumber,
            CnhType = cnhType,
            CreatedAtUtc = Mottu.Rentals.Application.Common.Time.BrazilTime.Now()
        };
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
        return new CourierResponse(entity.Id, entity.Identifier, entity.Name, entity.Cnpj, DateOnly.FromDateTime(entity.BirthDate), entity.CnhNumber, entity.CnhType.ToString(), entity.CnhImagePath);
    }

    public async Task<CourierResponse?> UploadCnhAsync(Guid id, string fileName, string contentType, Stream content, CancellationToken ct)
    {
        if (!IsAllowed(contentType))
            throw new InvalidOperationException("Only png or bmp allowed.");

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return null;

        var path = await _storage.SaveAsync($"{id}_cnh{GetExtension(contentType)}", content, ct);
        entity.CnhImagePath = path;
        entity.UpdatedAtUtc = Mottu.Rentals.Application.Common.Time.BrazilTime.Now();
        await _repo.SaveChangesAsync(ct);

        return new CourierResponse(entity.Id, entity.Identifier, entity.Name, entity.Cnpj, DateOnly.FromDateTime(entity.BirthDate), entity.CnhNumber, entity.CnhType.ToString(), entity.CnhImagePath);
    }

    private static bool IsAllowed(string contentType)
        => string.Equals(contentType, "image/png", StringComparison.OrdinalIgnoreCase)
        || string.Equals(contentType, "image/bmp", StringComparison.OrdinalIgnoreCase);

    private static string GetExtension(string contentType)
        => string.Equals(contentType, "image/png", StringComparison.OrdinalIgnoreCase) ? ".png" : ".bmp";
}


