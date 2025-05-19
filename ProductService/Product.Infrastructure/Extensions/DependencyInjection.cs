
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Product.Application.Interfaces;
using Product.Application.Services;
using Product.Domain.Interfaces;
using Product.Infrastructure.Data;
using Product.Infrastructure.Repositories;

namespace Product.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddProductInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ProductDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("Postgres")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
