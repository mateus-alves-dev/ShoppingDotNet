using Cart.Application.Services;
using Cart.Domain.Interfaces;
using Cart.Application.Interfaces; // For IProductServiceHttpClient
using Cart.Infrastructure.HttpClients; // For ProductServiceHttpClient
using Polly;
using Polly.Extensions.Http;
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

        // Add HttpClient for ProductService
        services.AddHttpClient<IProductServiceHttpClient, ProductServiceHttpClient>((serviceProvider, client) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var productServiceUrl = configuration["ProductServiceBaseUrl"];
            if (string.IsNullOrEmpty(productServiceUrl))
            {
                // Fallback or throw exception if not configured - for now, let's log and use a placeholder that will likely fail if not set
                Console.WriteLine("Warning: ProductServiceBaseUrl is not configured in appsettings.json. HttpClient may not work correctly.");
                // throw new InvalidOperationException("ProductServiceBaseUrl is not configured.");
                client.BaseAddress = new Uri("http://localhost:5003"); // Placeholder, will be product-api:8080 in Docker
            }
            else
            {
                client.BaseAddress = new Uri(productServiceUrl);
            }
            client.Timeout = TimeSpan.FromSeconds(5); // Example timeout
        })
        .AddPolicyHandler(GetRetryPolicy()) // Add basic retry policy
        .AddPolicyHandler(GetCircuitBreakerPolicy()); // Add basic circuit breaker policy

        services.AddSingleton<RabbitMqPublisher>();

        return services;
    }

    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handles HttpRequestException, 5XX and 408
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound) // Optional: Retry on 404 Not Found
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Log retry attempt
                    Console.WriteLine($"Retrying HTTP request... attempt {retryAttempt}, outcome: {outcome.Result?.StatusCode}, due to: {outcome.Exception?.Message}");
                }
            );
    }

    static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
                onBreak: (result, timeSpan, context) => 
                {
                    Console.WriteLine("Circuit breaker opened.");
                },
                onReset: (context) =>
                {
                    Console.WriteLine("Circuit breaker reset.");
                },
                onHalfOpen: () => 
                {
                    Console.WriteLine("Circuit breaker half-open.");
                }
            );
    }
}
