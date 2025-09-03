using System.Text.Json.Serialization;

namespace Mottu.Rentals.Contracts.Rentals;

public record RentalCreateRequest(
    [property: JsonPropertyName("motorcycleId")] Guid MotorcycleId,
    [property: JsonPropertyName("courierId")] Guid CourierId,
    [property: JsonPropertyName("plan")] int Plan
);

public record RentalReturnRequest(
    [property: JsonPropertyName("endDate")] DateOnly EndDate
);

public record RentalResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("motorcycleId")] Guid MotorcycleId,
    [property: JsonPropertyName("courierId")] Guid CourierId,
    [property: JsonPropertyName("plan")] int Plan,
    [property: JsonPropertyName("startDate")] DateOnly StartDate,
    [property: JsonPropertyName("expectedEndDate")] DateOnly ExpectedEndDate,
    [property: JsonPropertyName("endDate")] DateOnly? EndDate,
    [property: JsonPropertyName("dailyPrice")] decimal DailyPrice,
    [property: JsonPropertyName("totalPrice")] decimal? TotalPrice
);


