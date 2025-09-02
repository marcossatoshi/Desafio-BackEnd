using System.Text.Json.Serialization;

namespace Mottu.Rentals.Contracts.Motorcycles;

public record MotorcycleCreatedNotificationDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("motorcycleId")] Guid MotorcycleId,
    [property: JsonPropertyName("year")] int Year,
    [property: JsonPropertyName("publishedAtUtc")] DateTime PublishedAtUtc
);


