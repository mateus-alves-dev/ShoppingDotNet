
using System.Text.Json;

using Cart.Domain.Entities;

using StackExchange.Redis;

namespace Cart.Infrastructure.Cache;

public class RedisCartCache
{
    private readonly IDatabase _db;
    public RedisCartCache(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<Domain.Entities.Cart?> GetCartAsync(Guid userId)
    {
        var value = await _db.StringGetAsync(userId.ToString());
        if (value.IsNullOrEmpty) return null;
        return JsonSerializer.Deserialize<Domain.Entities.Cart>(value!);
    }

    public async Task SetCartAsync(Domain.Entities.Cart cart)
    {
        var data = JsonSerializer.Serialize(cart);
        await _db.StringSetAsync(cart.UserId.ToString(), data);
    }
    public async Task DeleteCartAsync(Guid userid)
    {
        await _db.KeyDeleteAsync(userid.ToString());
    }
}
