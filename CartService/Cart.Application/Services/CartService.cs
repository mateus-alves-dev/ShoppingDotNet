
using Cart.Application.DTOs;
using Cart.Application.Interfaces;
using Cart.Domain.Entities;
using Cart.Domain.Interfaces;

namespace Cart.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }
    public async Task AddItemAsync(Guid userId, CartItemDto item)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId) ?? new Domain.Entities.Cart { UserId = userId };

        cart.AddItem(item.ProductId, item.Quantity, item.UnitPrice);

        await _cartRepository.AddOrUpdateAsync(cart);
    }

    public async Task ClearCartAsync(Guid userId)
    {
        await _cartRepository.DeleteAsync(userId);
    }

    public async Task<CartDto?> GetCartAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null) return null;
        return ToDto(cart);
    }

    public async Task RemoveItemAsync(Guid userId, Guid productId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null) return;
        cart.RemoveItem(productId);
        await _cartRepository.AddOrUpdateAsync(cart);
    }

    private static CartDto ToDto(Cart.Domain.Entities.Cart cart)
    {
        return new CartDto
        {
            UserId = cart.UserId,
            Items = cart.Items.Select(i => new CartItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            TotalAmount = cart.TotalAmount
        };
    }
}
