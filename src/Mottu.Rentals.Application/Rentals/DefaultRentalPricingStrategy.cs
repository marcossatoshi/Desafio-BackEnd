using Mottu.Rentals.Application.Abstractions;
using Mottu.Rentals.Domain.Entities;
using Mottu.Rentals.Domain.Enums;

namespace Mottu.Rentals.Application.Rentals;

public sealed class DefaultRentalPricingStrategy : IRentalPricingStrategy
{
    public decimal DetermineDailyPrice(PlanType plan) => plan switch
    {
        PlanType.Days7 => 30m,
        PlanType.Days15 => 28m,
        PlanType.Days30 => 22m,
        PlanType.Days45 => 20m,
        PlanType.Days50 => 18m,
        _ => throw new InvalidOperationException("Invalid plan.")
    };

    public decimal CalculateTotalOnReturn(Rental rental, DateOnly endDate)
    {
        if (endDate < rental.ExpectedEndDate)
        {
            var usedDays = (endDate.DayNumber - rental.StartDate.DayNumber);
            var totalDiarias = usedDays * rental.DailyPrice;
            var remainingDays = (rental.ExpectedEndDate.DayNumber - endDate.DayNumber);
            var finePerc = rental.Plan == PlanType.Days7 ? 0.20m : rental.Plan == PlanType.Days15 ? 0.40m : 0m;
            var fine = remainingDays * rental.DailyPrice * finePerc;
            return totalDiarias + fine;
        }

        if (endDate > rental.ExpectedEndDate)
        {
            var extraDays = (endDate.DayNumber - rental.ExpectedEndDate.DayNumber);
            return ((int)rental.Plan * rental.DailyPrice) + (extraDays * 50m);
        }

        return ((int)rental.Plan * rental.DailyPrice);
    }
}


