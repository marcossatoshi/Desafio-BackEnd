using Microsoft.AspNetCore.Mvc;
using Mottu.Rentals.Application.Motorcycles;
using Mottu.Rentals.Contracts.Motorcycles;
using Mottu.Rentals.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mottu.Rentals.Api.Endpoints;

public static class MotorcyclesEndpoints
{
    public static IEndpointRouteBuilder MapMotorcyclesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/motorcycles");
        group.WithTags("Motorcycles");
        group.WithGroupName("motorcycles");

        group.MapPost("", async ([FromServices] IMotorcycleService service, [FromBody] MotorcycleCreateRequest request, CancellationToken ct) =>
        {
            try
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/motorcycles/{created.Id}", created);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        });

        group.MapGet("", async ([FromServices] IMotorcycleService service, [FromQuery] string? plate, CancellationToken ct) =>
        {
            var list = await service.ListAsync(plate, ct);
            return Results.Ok(list);
        });

        group.MapGet("{identifier}", async ([FromServices] RentalsDbContext db, [FromRoute] string identifier, CancellationToken ct) =>
        {
            var m = await db.Motorcycles.AsNoTracking().FirstOrDefaultAsync(x => x.Identifier == identifier, ct);
            if (m is null) return Results.NotFound();
            var dto = new MotorcycleResponse(m.Id, m.Identifier, m.Year, m.Model, m.Plate, m.CreatedAtUtc);
            return Results.Ok(dto);
        });

        group.MapPut("{identifier}/plate", async ([FromServices] RentalsDbContext db, [FromServices] IMotorcycleService service, [FromRoute] string identifier, [FromBody] MotorcycleUpdatePlateRequest request, CancellationToken ct) =>
        {
            var entity = await db.Motorcycles.FirstOrDefaultAsync(x => x.Identifier == identifier, ct);
            if (entity is null) return Results.NotFound();
            try
            {
                var updated = await service.UpdatePlateAsync(entity.Id, request, ct);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        });

        group.MapPut("{id:guid}/plate", async ([FromServices] IMotorcycleService service, [FromRoute] Guid id, [FromBody] MotorcycleUpdatePlateRequest request, CancellationToken ct) =>
        {
            try
            {
                var updated = await service.UpdatePlateAsync(id, request, ct);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        });

        group.MapDelete("{id:guid}", async ([FromServices] IMotorcycleService service, [FromRoute] Guid id, CancellationToken ct) =>
        {
            var ok = await service.DeleteAsync(id, ct);
            return ok ? Results.NoContent() : Results.Conflict(new { message = "Motorcycle has rentals or does not exist." });
        });

        return app;
    }
}


