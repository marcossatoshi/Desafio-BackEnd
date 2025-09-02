namespace Mottu.Rentals.Contracts.Events;

public record MotorcycleCreatedEvent(Guid Id, int Year, string Model, string Plate, DateTime CreatedAtUtc);


