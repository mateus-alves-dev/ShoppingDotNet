using Microsoft.EntityFrameworkCore;


namespace Order.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    public DbSet<Domain.Entities.Order> Orders { get; set; }

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.Order>().HasKey(o => o.Id);

        modelBuilder.Entity<Domain.Entities.Order>()
            .OwnsMany(o => o.Items, a =>
            {
                a.WithOwner().HasForeignKey("OrderId");
                a.Property<Guid>("Id");
                a.HasKey("Id");
            });

        base.OnModelCreating(modelBuilder);
    }
}
