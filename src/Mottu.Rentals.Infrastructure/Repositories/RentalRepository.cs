using Microsoft.EntityFrameworkCore;
using Mottu.Rentals.Application.Abstractions;
using Mottu.Rentals.Domain.Entities;
using Mottu.Rentals.Infrastructure.Persistence;

namespace Mottu.Rentals.Infrastructure.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly RentalsDbContext _db;
    public RentalRepository(RentalsDbContext db) => _db = db;

    public async Task AddAsync(Rental entity, CancellationToken ct)
        => await _db.Rentals.AddAsync(entity, ct);

    public Task<Rental?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Rentals.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> ExistsActiveRentalForMotorcycleAsync(Guid motorcycleId, CancellationToken ct)
        => _db.Rentals.AnyAsync(x => x.MotorcycleId == motorcycleId && x.EndDate == null, ct);

    public Task<bool> ExistsActiveRentalForCourierAsync(Guid courierId, CancellationToken ct)
        => _db.Rentals.AnyAsync(x => x.CourierId == courierId && x.EndDate == null, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}


