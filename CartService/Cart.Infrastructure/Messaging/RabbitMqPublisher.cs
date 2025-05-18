using System.Text;
using System.Text.Json;

using Cart.Application.DTOs;

using Microsoft.Extensions.Configuration;

using RabbitMQ.Client;

namespace Cart.Infrastructure.Messaging;

public class RabbitMqPublisher : IAsyncDisposable
{
    private readonly IConfiguration _config;
    private readonly IConnection _connection;

    public RabbitMqPublisher(IConfiguration config)
    {
        _config = config;

        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"]!,
            UserName = _config["RabbitMQ:User"]!,
            Password = _config["RabbitMQ:Pass"]!
        };

        // Como só usamos uma conexão e canal por publicação, usamos aqui diretamente.
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }

    public async Task PublishCartCheckout(CartDto cart)
    {
        Console.WriteLine("testeeeeee" + JsonSerializer.Serialize(cart));
        using var channel = await _connection.CreateChannelAsync();

        // Garante que a fila existe (idempotente)
        await channel.QueueDeclareAsync(
            queue: "cart-checkout",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cart));

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "cart-checkout",
            body: body
        );
        Console.WriteLine(body);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
