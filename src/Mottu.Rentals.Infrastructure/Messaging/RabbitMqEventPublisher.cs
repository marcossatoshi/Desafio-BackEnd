using MassTransit;
using Mottu.Rentals.Application.Abstractions;

namespace Mottu.Rentals.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public RabbitMqEventPublisher(IPublishEndpoint publishEndpoint)
        => _publishEndpoint = publishEndpoint;

    public Task PublishAsync<T>(T message, string routingKey = "", CancellationToken cancellationToken = default)
        => _publishEndpoint.Publish(message, cancellationToken);
}


