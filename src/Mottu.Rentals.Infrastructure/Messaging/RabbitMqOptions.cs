namespace Mottu.Rentals.Infrastructure.Messaging;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Exchange { get; set; } = "mottu.events";
    public string Queue { get; set; } = "motorcycle.created";
    public string RoutingKey { get; set; } = "motorcycle.created";
}


