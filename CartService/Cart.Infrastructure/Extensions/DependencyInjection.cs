using Cart.Application.Interfaces;
using Cart.Application.Services;
using Cart.Domain.Interfaces;
using Cart.Infrastructure.Cache;
using Cart.Infrastructure.Data;
using Cart.Infrastructure.Messaging;
using Cart.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql.Replication;

using StackExchange.Redis;

namespace Cart.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddCartInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<CartDbContext>(options => options.UseNpgsql(config.GetConnectionString("Postgres")));

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));

        services.AddScoped<RedisCartCache>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartService, CartService>();
        services.AddSingleton<RabbitMqPublisher>();

        return services;
    }
}
