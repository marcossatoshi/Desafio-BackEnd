using Microsoft.AspNetCore.Mvc;
using Mottu.Rentals.Application.Rentals;
using Mottu.Rentals.Contracts.Rentals;
using Mottu.Rentals.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mottu.Rentals.Api.Endpoints;

public static class RentalsEndpoints
{
    public static IEndpointRouteBuilder MapRentalsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/rentals");
        group.WithTags("Rentals");
        group.WithGroupName("rentals");

        group.MapPost("", async ([FromServices] IRentalService service, [FromBody] RentalCreateRequest request, CancellationToken ct) =>
        {
            try
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/rentals/{created.Id}", created);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        });

        group.MapPost("{id:guid}/return", async ([FromServices] IRentalService service, [FromRoute] Guid id, [FromBody] RentalReturnRequest request, CancellationToken ct) =>
        {
            var updated = await service.ReturnAsync(id, request, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        group.MapGet("{id:guid}", async ([FromServices] RentalsDbContext db, [FromRoute] Guid id, CancellationToken ct) =>
        {
            var r = await db.Rentals.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (r is null) return Results.NotFound();
            var dto = new RentalResponse(r.Id, r.Identifier, r.MotorcycleId, r.CourierId, (int)r.Plan, r.StartDate, r.ExpectedEndDate, r.EndDate, r.DailyPrice, r.TotalPrice);
            return Results.Ok(dto);
        });

        return app;
    }
}


