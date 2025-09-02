using Mottu.Rentals.Domain.Entities;
using Mottu.Rentals.Domain.Enums;

namespace Mottu.Rentals.Application.Abstractions;

public interface IRentalPricingStrategy
{
    decimal DetermineDailyPrice(PlanType plan);

    decimal CalculateTotalOnReturn(Rental rental, DateOnly endDate);
}


