using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Order.Infrastructure.Data;
using Order.Infrastructure.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("Postgres")));

        services.AddHostedService<RabbitMqOrderConsumer>();

        return services;
    }
}
