using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Mottu.Rentals.Contracts.Couriers;
using Mottu.Rentals.Contracts.Rentals;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Mottu.Rentals.Api.Swagger;

public sealed class SwaggerDefaultsSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CourierCreateRequest))
        {
            if (schema.Properties.TryGetValue("cnhType", out var cnhProp))
            {
                cnhProp.Default = new OpenApiString("A");
                AppendAllowedValues(cnhProp, new[] { new OpenApiString("A"), new OpenApiString("B"), new OpenApiString("AB") },
                    "Allowed values: A, B, AB. Default: A.");
            }
        }

        if (context.Type == typeof(RentalCreateRequest))
        {
            if (schema.Properties.TryGetValue("plan", out var planProp))
            {
                planProp.Default = new OpenApiInteger(7);
                AppendAllowedValues(planProp, new IOpenApiAny[]
                {
                    new OpenApiInteger(7), new OpenApiInteger(15), new OpenApiInteger(30), new OpenApiInteger(45), new OpenApiInteger(50)
                }, "Allowed values: 7, 15, 30, 45, 50. Default: 7.");
            }
        }
    }

    private static void AppendAllowedValues(OpenApiSchema prop, IEnumerable<IOpenApiAny> values, string description)
    {
        prop.Enum ??= new List<IOpenApiAny>();
        foreach (var v in values)
        {
            prop.Enum.Add(v);
        }
        prop.Description = string.IsNullOrWhiteSpace(prop.Description)
            ? description
            : $"{prop.Description} {description}";
    }
}
