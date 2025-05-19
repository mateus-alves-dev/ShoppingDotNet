using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Product.Infrastructure.Data;

public class ProductDbContextFactory : IDesignTimeDbContextFactory<ProductDbContext>
{
    public ProductDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProductDbContext>();
        var connectionString = "Host=localhost;Port=5432;Username=dev;Password=devpass;Database=productdb";
        optionsBuilder.UseNpgsql(connectionString);
        return new ProductDbContext(optionsBuilder.Options);
    }
}
