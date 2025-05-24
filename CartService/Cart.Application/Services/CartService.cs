
using Cart.Application.DTOs;
using Cart.Application.Interfaces; // For IProductServiceHttpClient
using Cart.Domain.Entities;
using Cart.Domain.Interfaces;
// Potentially: using Microsoft.Extensions.Logging; if logging is added here

namespace Cart.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductServiceHttpClient _productServiceHttpClient;
    // private readonly ILogger<CartService> _logger; // If adding logging

    public CartService(ICartRepository cartRepository, IProductServiceHttpClient productServiceHttpClient /*, ILogger<CartService> logger */)
    {
        _cartRepository = cartRepository;
        _productServiceHttpClient = productServiceHttpClient;
        // _logger = logger;
    }
    public async Task AddItemAsync(Guid userId, CartItemDto itemDto) // itemDto here will only have ProductId and Quantity from client
    {
        if (itemDto.ProductId == Guid.Empty || itemDto.Quantity <= 0)
        {
            // Or throw ArgumentException
            // _logger.LogWarning("Invalid ProductId or Quantity for AddItemAsync.");
            throw new ArgumentException("ProductId must be valid and Quantity must be greater than 0.");
        }

        var product = await _productServiceHttpClient.GetProductByIdAsync(itemDto.ProductId);

        if (product == null)
        {
            // _logger.LogWarning("Product with ID {ProductId} not found via ProductServiceHttpClient.", itemDto.ProductId);
            throw new KeyNotFoundException($"Product with ID {itemDto.ProductId} not found."); // Or a custom ProductNotFoundException
        }

        var cart = await _cartRepository.GetByUserIdAsync(userId) ?? new Domain.Entities.Cart { UserId = userId };

        // Call the updated domain entity's AddItem method
        cart.AddItem(
            productId: product.Id, // or itemDto.ProductId
            quantity: itemDto.Quantity,
            unitPrice: product.Price,
            productName: product.Name,
            productImageUrl: product.ImageUrl
        );

        await _cartRepository.AddOrUpdateAsync(cart);
        // _logger.LogInformation("Item {ProductId} added/updated in cart for user {UserId}.", itemDto.ProductId, userId);
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
                UnitPrice = i.UnitPrice, // This is the price at the time it was added/updated
                ProductName = i.ProductName, // New mapping
                ProductImageUrl = i.ProductImageUrl // New mapping
            }).ToList(),
            TotalAmount = cart.TotalAmount
        };
    }
}
