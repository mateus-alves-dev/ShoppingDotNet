using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Order.Application.DTOs;
using Order.Domain.Entities;
using Order.Infrastructure.Data;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Order.Infrastructure.Messaging;

public class RabbitMqOrderConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private IConnection? _connection;

    public RabbitMqOrderConsumer(IServiceScopeFactory scopeFactory, IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"],
            UserName = _config["RabbitMQ:User"],
            Password = _config["RabbitMQ:Pass"]
        };

        _connection = await factory.CreateConnectionAsync();

        using var channel = await _connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "cart-checkout",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var cart = JsonSerializer.Deserialize<CartDto>(json);
            Console.WriteLine(cart);
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            var order = new Order.Domain.Entities.Order
            {
                Id = Guid.NewGuid(),
                UserId = cart!.UserId,
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();
        };

        var data = await channel.BasicConsumeAsync(queue: "cart-checkout", autoAck: true, consumer: consumer);
        Console.WriteLine(data);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _connection?.Dispose();
        base.Dispose();
    }
}
