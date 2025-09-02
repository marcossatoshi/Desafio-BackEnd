using System.Text.Json.Serialization;

namespace Mottu.Rentals.Contracts.Couriers;

public record CourierCreateRequest(
    [property: JsonPropertyName("identifier")] string Identifier,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("cnpj")] string Cnpj,
    [property: JsonPropertyName("birthDate")] DateOnly BirthDate,
    [property: JsonPropertyName("cnhNumber")] string CnhNumber,
    [property: JsonPropertyName("cnhType")] string CnhType
);

public record CourierResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("identifier")] string Identifier,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("cnpj")] string Cnpj,
    [property: JsonPropertyName("birthDate")] DateOnly BirthDate,
    [property: JsonPropertyName("cnhNumber")] string CnhNumber,
    [property: JsonPropertyName("cnhType")] string CnhType,
    [property: JsonPropertyName("cnhImagePath")] string? CnhImagePath
);


