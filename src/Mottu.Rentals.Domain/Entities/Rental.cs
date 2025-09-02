using Mottu.Rentals.Domain.Enums;

namespace Mottu.Rentals.Domain.Entities;

public class Rental
{
    public Guid Id { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public Guid MotorcycleId { get; set; }
    public Guid CourierId { get; set; }
    public PlanType Plan { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateOnly StartDate { get; set; }
    public DateOnly ExpectedEndDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal DailyPrice { get; set; }
    public decimal? TotalPrice { get; set; }
}


