using Microsoft.EntityFrameworkCore;

using Product.Domain.Interfaces;
using Product.Infrastructure.Data;

namespace Product.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _db;
    public ProductRepository(ProductDbContext db)
    {
        _db = db;
    }
    public async Task AddAsync(Domain.Entities.Product product)
    {
        await _db.Products.AddAsync(product);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return;
        _db.Remove(product);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Domain.Entities.Product>> GetAllAsync()
    {
        return await _db.Products.AsNoTracking().ToListAsync();
    }

    public Task<Domain.Entities.Product?> GetByIdAsync(Guid id)
    {
        return _db.Products.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task UpdateAsync(Domain.Entities.Product product)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }
}
