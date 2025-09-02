using MassTransit;
using Microsoft.EntityFrameworkCore;
using Mottu.Rentals.Contracts.Events;
using Mottu.Rentals.Infrastructure.Entities;
using Mottu.Rentals.Infrastructure.Persistence;

namespace Mottu.Rentals.Infrastructure.Messaging;

public class MotorcycleCreatedConsumer : IConsumer<MotorcycleCreatedEvent>
{
    private readonly RentalsDbContext _db;
    public MotorcycleCreatedConsumer(RentalsDbContext db) => _db = db;

    public async Task Consume(ConsumeContext<MotorcycleCreatedEvent> context)
    {
        var msg = context.Message;
        if (msg.Year == 2024)
        {
            var exists = await _db.Set<MotorcycleCreatedNotification>()
                .AsNoTracking()
                .AnyAsync(x => x.MotorcycleId == msg.Id, context.CancellationToken);
            if (!exists)
            {
                _db.Set<MotorcycleCreatedNotification>().Add(new MotorcycleCreatedNotification
                {
                    Id = Guid.NewGuid(),
                    MotorcycleId = msg.Id,
                    Year = msg.Year,
                    PublishedAtUtc = DateTime.UtcNow
                });
                await _db.SaveChangesAsync(context.CancellationToken);
            }
        }
    }
}


