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
        var planDays = (int)rental.Plan;

        if (endDate < rental.ExpectedEndDate)
        {
            // Early return
            var finePerc = rental.Plan == PlanType.Days7 ? 0.20m : 0.40m;

            if (endDate <= rental.StartDate)
            {
                // Returned before or on the start date: no daily usage, full plan fine
                return planDays * rental.DailyPrice * finePerc;
            }

            var usedDays = Math.Max(0, endDate.DayNumber - rental.StartDate.DayNumber);
            var remainingDays = Math.Max(0, rental.ExpectedEndDate.DayNumber - endDate.DayNumber);
            var totalDiarias = usedDays * rental.DailyPrice;
            var fine = remainingDays * rental.DailyPrice * finePerc;
            return totalDiarias + fine;
        }

        if (endDate > rental.ExpectedEndDate)
        {
            // Late return
            var extraDays = endDate.DayNumber - rental.ExpectedEndDate.DayNumber;
            return (planDays * rental.DailyPrice) + (extraDays * 50m);
        }

        // On-time return
        return planDays * rental.DailyPrice;
    }
}


