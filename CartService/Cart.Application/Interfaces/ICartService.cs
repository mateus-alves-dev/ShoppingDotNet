using Cart.Application.DTOs;
using Cart.Domain.Entities;

namespace Cart.Application.Interfaces;

public interface ICartService
{
    Task<CartDto?> GetCartAsync(Guid userId);
    Task AddItemAsync(Guid userId, CartItemDto item);
    Task RemoveItemAsync(Guid userId, Guid productId);
    Task ClearCartAsync(Guid userId);
}
