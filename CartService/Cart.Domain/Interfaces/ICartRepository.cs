namespace Cart.Domain.Interfaces;

using Cart.Domain.Entities;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid UserId);
    Task AddOrUpdateAsync(Cart cart);
    Task DeleteAsync(Guid userId);
}
