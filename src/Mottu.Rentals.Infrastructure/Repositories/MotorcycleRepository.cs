using Microsoft.EntityFrameworkCore;
using Mottu.Rentals.Application.Abstractions;
using Mottu.Rentals.Domain.Entities;
using Mottu.Rentals.Infrastructure.Persistence;

namespace Mottu.Rentals.Infrastructure.Repositories;

public class MotorcycleRepository : IMotorcycleRepository
{
    private readonly RentalsDbContext _db;
    public MotorcycleRepository(RentalsDbContext db) => _db = db;

    public Task<bool> ExistsPlateAsync(string plate, CancellationToken ct)
        => _db.Motorcycles.AnyAsync(x => x.Plate == plate, ct);

    public async Task AddAsync(Motorcycle entity, CancellationToken ct)
        => await _db.Motorcycles.AddAsync(entity, ct);

    public async Task<List<Motorcycle>> ListAsync(string? plate, CancellationToken ct)
    {
        var query = _db.Motorcycles.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(plate))
        {
            var term = $"%{plate.Trim().ToLower()}%";
            query = query.Where(x => EF.Functions.Like(x.Plate.ToLower(), term));
        }
        return await query.OrderBy(x => x.Plate).ToListAsync(ct);
    }

    public Task<Motorcycle?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Motorcycles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> PlateInUseByOtherAsync(Guid id, string plate, CancellationToken ct)
        => _db.Motorcycles.AnyAsync(x => x.Plate == plate && x.Id != id, ct);

    public Task<bool> HasRentalAsync(Guid motorcycleId, CancellationToken ct)
        => _db.Rentals.AnyAsync(r => r.MotorcycleId == motorcycleId, ct);

    public Task RemoveAsync(Motorcycle entity, CancellationToken ct)
    {
        _db.Motorcycles.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}


