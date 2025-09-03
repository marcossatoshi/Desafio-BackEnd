namespace Mottu.Rentals.Infrastructure.Entities;

public class MotorcycleCreatedNotification
{
    public Guid Id { get; set; }
    public Guid MotorcycleId { get; set; }
    public int Year { get; set; }
    public DateTime PublishedAtUtc { get; set; }
}


