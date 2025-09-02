using Mottu.Rentals.Domain.Enums;

namespace Mottu.Rentals.Domain.Entities;

public class Courier
{
    public Guid Id { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string CnhNumber { get; set; } = string.Empty;
    public CnhType CnhType { get; set; }
    public string? CnhImagePath { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}


