using Microsoft.EntityFrameworkCore;

namespace Cart.Infrastructure.Data;

public class CartDbContext : DbContext
{

    public DbSet<Domain.Entities.Cart> Carts { get; set; }
    public CartDbContext(DbContextOptions<CartDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.Cart>().HasKey(c => c.Id);
        modelBuilder.Entity<Domain.Entities.Cart>().HasIndex(c => c.UserId).IsUnique();

        modelBuilder.Entity<Domain.Entities.Cart>().OwnsMany(c => c.Items, a =>
        {
            a.WithOwner().HasForeignKey("CartId");
            a.Property<Guid>("Id");
            a.HasKey("Id");
        });
        base.OnModelCreating(modelBuilder);
    }
}
