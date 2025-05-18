
using Cart.Domain.Interfaces;
using Cart.Infrastructure.Cache;
using Cart.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace Cart.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CartDbContext _db;
    private readonly RedisCartCache _cache;

    public CartRepository(CartDbContext db, RedisCartCache cache)
    {
        _db = db;
        _cache = cache;
    }
    public async Task<Domain.Entities.Cart?> GetByUserIdAsync(Guid UserId)
    {
        var cached = await _cache.GetCartAsync(UserId);
        if (cached != null) return cached;

        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync();

        if (cart != null) await _cache.SetCartAsync(cart);

        return cart;
    }
    public async Task AddOrUpdateAsync(Domain.Entities.Cart cart)
    {
        var exists = await _db.Carts.AnyAsync(c => c.UserId == cart.UserId);
        if (exists) _db.Carts.Update(cart);
        else await _db.Carts.AddAsync(cart);

        await _db.SaveChangesAsync();
        await _cache.SetCartAsync(cart);
    }

    public async Task DeleteAsync(Guid userId)
    {
        var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart != null)
        {
            _db.Carts.Remove(cart);
            await _db.SaveChangesAsync();
        }
        await _cache.DeleteCartAsync(userId);
    }

}
