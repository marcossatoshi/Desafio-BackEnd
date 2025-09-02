using Microsoft.EntityFrameworkCore;
using Mottu.Rentals.Application.Abstractions;
using Mottu.Rentals.Domain.Entities;
using Mottu.Rentals.Infrastructure.Persistence;

namespace Mottu.Rentals.Infrastructure.Repositories;

public class CourierRepository : ICourierRepository
{
    private readonly RentalsDbContext _db;
    public CourierRepository(RentalsDbContext db) => _db = db;

    public Task<bool> ExistsCnpjAsync(string cnpj, CancellationToken ct)
        => _db.Couriers.AnyAsync(x => x.Cnpj == cnpj, ct);

    public Task<bool> ExistsCnhNumberAsync(string cnhNumber, CancellationToken ct)
        => _db.Couriers.AnyAsync(x => x.CnhNumber == cnhNumber, ct);

    public async Task AddAsync(Courier entity, CancellationToken ct)
        => await _db.Couriers.AddAsync(entity, ct);

    public Task<Courier?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Couriers.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}


