using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mottu.Rentals.Application.Abstractions;
using Mottu.Rentals.Infrastructure.Messaging;
using Mottu.Rentals.Infrastructure.Repositories;

namespace Mottu.Rentals.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddMassTransit(x =>
		{
			x.AddConsumer<MotorcycleCreatedConsumer>();
			var useInMemory = string.Equals(configuration["UseMassTransitInMemory"], "true", StringComparison.OrdinalIgnoreCase);
			if (useInMemory)
			{
				x.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); });
			}
			else
			{
				x.UsingRabbitMq((context, cfg) =>
				{
					var host = configuration["RabbitMq:HostName"] ?? "localhost";
					var user = configuration["RabbitMq:UserName"] ?? "guest";
					var pass = configuration["RabbitMq:Password"] ?? "guest";
					var portStr = configuration["RabbitMq:Port"];
					var port = 5672;
					if (!string.IsNullOrWhiteSpace(portStr) && int.TryParse(portStr, out var parsed)) port = parsed;
					var uri = new Uri($"rabbitmq://{host}:{port}/");
					cfg.Host(uri, h =>
					{
						h.Username(user);
						h.Password(pass);
					});
					cfg.ConfigureEndpoints(context);
				});
			}
		});

		services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
		services.AddScoped<IMotorcycleRepository, MotorcycleRepository>();
		services.AddScoped<ICourierRepository, CourierRepository>();
		services.AddScoped<IRentalRepository, RentalRepository>();
		services.AddScoped<Mottu.Rentals.Application.Abstractions.IRentalPricingStrategy, Mottu.Rentals.Application.Rentals.DefaultRentalPricingStrategy>();
		services.AddSingleton<Mottu.Rentals.Application.Abstractions.IFileStorage, Mottu.Rentals.Infrastructure.Storage.LocalFileStorage>();

		// Decorators
		services.AddMemoryCache();


		services.AddScoped<Mottu.Rentals.Application.Motorcycles.MotorcycleService>();
		services.AddScoped<Mottu.Rentals.Application.Motorcycles.IMotorcycleService>(sp =>
		{
			var core = sp.GetRequiredService<Mottu.Rentals.Application.Motorcycles.MotorcycleService>();
			var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Mottu.Rentals.Application.Motorcycles.LoggingMotorcycleServiceDecorator>>();
			var cached = new Mottu.Rentals.Application.Motorcycles.CachingMotorcycleServiceDecorator(
				core,
				sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
				sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Mottu.Rentals.Application.Motorcycles.CachingMotorcycleServiceDecorator>>());
			return new Mottu.Rentals.Application.Motorcycles.LoggingMotorcycleServiceDecorator(cached, logger);
		});

		services.AddScoped<Mottu.Rentals.Application.Couriers.CourierService>();
		services.AddScoped<Mottu.Rentals.Application.Couriers.ICourierService>(sp =>
			new Mottu.Rentals.Application.Couriers.LoggingCourierServiceDecorator(
				sp.GetRequiredService<Mottu.Rentals.Application.Couriers.CourierService>(),
				sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Mottu.Rentals.Application.Couriers.LoggingCourierServiceDecorator>>()));

		services.AddScoped<Mottu.Rentals.Application.Rentals.RentalService>();
		services.AddScoped<Mottu.Rentals.Application.Rentals.IRentalService>(sp =>
			new Mottu.Rentals.Application.Rentals.LoggingRentalServiceDecorator(
				sp.GetRequiredService<Mottu.Rentals.Application.Rentals.RentalService>(),
				sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Mottu.Rentals.Application.Rentals.LoggingRentalServiceDecorator>>()));

		return services;
	}
}


