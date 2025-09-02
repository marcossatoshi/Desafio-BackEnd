using System.Text.Json.Serialization;

namespace Mottu.Rentals.Contracts.Motorcycles;

public record MotorcycleCreateRequest(
    [property: JsonPropertyName("identifier")] string Identifier,
    [property: JsonPropertyName("year")] int Year,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("plate")] string Plate
);

public record MotorcycleUpdatePlateRequest(
    [property: JsonPropertyName("plate")] string Plate
);

public record MotorcycleResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("identifier")] string Identifier,
    [property: JsonPropertyName("year")] int Year,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("plate")] string Plate,
    [property: JsonPropertyName("createdAtUtc")] DateTime CreatedAtUtc
);


