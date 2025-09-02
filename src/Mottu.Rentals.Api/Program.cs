using Microsoft.EntityFrameworkCore;
using Mottu.Rentals.Api.Endpoints;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mottu Rentals API",
        Version = "v1",
        Description = "Motorcycles, Couriers (CNH upload) and Rentals endpoints"
    });
    options.DocInclusionPredicate((docName, apiDesc) => true);
    options.SchemaFilter<Mottu.Rentals.Api.Swagger.SwaggerDefaultsSchemaFilter>();
});

builder.Services.AddDbContext<Mottu.Rentals.Infrastructure.Persistence.RentalsDbContext>(options =>
{
    var useInMemory = string.Equals(builder.Configuration["UseInMemoryEF"], "true", StringComparison.OrdinalIgnoreCase);
    if (useInMemory)
    {
        options.UseInMemoryDatabase("api-tests-db");
    }
    else
    {
        var cs = builder.Configuration.GetConnectionString("Postgres")
                 ?? builder.Configuration["MOTTU_POSTGRES_CONNECTION"]
                 ?? "Host=localhost;Port=5432;Database=mottu_rentals;Username=postgres;Password=postgres";
        options.UseNpgsql(cs);
    }
});

Mottu.Rentals.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);
builder.Services.AddScoped<Mottu.Rentals.Application.Motorcycles.IMotorcycleService, Mottu.Rentals.Application.Motorcycles.MotorcycleService>();
builder.Services.AddScoped<Mottu.Rentals.Application.Couriers.ICourierService, Mottu.Rentals.Application.Couriers.CourierService>();
builder.Services.AddScoped<Mottu.Rentals.Application.Rentals.IRentalService, Mottu.Rentals.Application.Rentals.RentalService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<Mottu.Rentals.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mottu Rentals API v1");
    c.RoutePrefix = "swagger";
    c.DisplayOperationId();
    c.DefaultModelsExpandDepth(0);
});

app.MapMotorcyclesEndpoints();
app.MapCouriersEndpoints();
app.MapRentalsEndpoints();

app.Run();

public partial class Program { }