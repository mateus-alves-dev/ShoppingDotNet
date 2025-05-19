namespace Product.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

using Product.Domain.Entities;
public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasKey(p => p.Id);
        base.OnModelCreating(modelBuilder);
    }
}
