using Microsoft.AspNetCore.Mvc;
using Mottu.Rentals.Application.Couriers;
using Mottu.Rentals.Contracts.Couriers;
using Mottu.Rentals.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mottu.Rentals.Api.Endpoints;

public static class CouriersEndpoints
{
    public static IEndpointRouteBuilder MapCouriersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/couriers");
        group.WithTags("Couriers");
        group.WithGroupName("couriers");

        group.MapPost("", async ([FromServices] ICourierService service, [FromBody] CourierCreateRequest request, CancellationToken ct) =>
        {
            try
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/couriers/{created.Id}", created);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        });

        group.MapPost("{identifier}/cnh", async ([FromServices] ICourierService service, [FromServices] RentalsDbContext db, [FromRoute] string identifier, IFormFile file, CancellationToken ct) =>
        {
            if (file is null || file.Length == 0)
                return Results.BadRequest(new { message = "File is required" });

            try
            {
                await using var stream = file.OpenReadStream();
                var courier = await db.Couriers.AsNoTracking().FirstOrDefaultAsync(x => x.Identifier == identifier, ct);
                if (courier is null) return Results.NotFound();
                var updated = await service.UploadCnhAsync(courier.Id, file.FileName, file.ContentType, stream, ct);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .DisableAntiforgery();

        return app;
    }
}


