using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cart.Infrastructure.Data;

public class CartDbContextFactory : IDesignTimeDbContextFactory<CartDbContext>
{
    public CartDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CartDbContext>();

        var connectionString = "Host=localhost;Port=5432;Username=dev;Password=devpass;Database=cartdb";
        optionsBuilder.UseNpgsql(connectionString);
        return new CartDbContext(optionsBuilder.Options);
    }
}
