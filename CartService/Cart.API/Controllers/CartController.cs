using Cart.Application.DTOs;
using Cart.Application.Interfaces;
using Cart.Infrastructure.Messaging;

using Microsoft.AspNetCore.Mvc;

namespace Cart.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly RabbitMqPublisher _publisher;

    public CartController(ICartService cartService, RabbitMqPublisher publisher)
    {
        _cartService = cartService;
        _publisher = publisher;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<CartDto>> GetCart(Guid userId)
    {
        var cart = await _cartService.GetCartAsync(userId);
        if (cart == null) return NotFound();
        return Ok(cart);
    }

    [HttpPost("{userId}/items")]
    public async Task<IActionResult> AddItem(Guid userId, [FromBody] CartItemDto item)
    {
        await _cartService.AddItemAsync(userId, item);
        return NoContent();
    }

    [HttpDelete("{userId}/items/{productId}")]
    public async Task<IActionResult> RemoveItem(Guid userId, Guid productId)
    {
        await _cartService.RemoveItemAsync(userId, productId);
        return NoContent();
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> ClearCart(Guid userId)
    {
        await _cartService.ClearCartAsync(userId);
        return NoContent();
    }

    [HttpPost("{userId}/checkout")]
    public async Task<IActionResult> Checkout(Guid userId)
    {
        var cart = await _cartService.GetCartAsync(userId);
        if (cart == null || !cart.Items.Any()) return BadRequest("Carrinho vazio");

        await _publisher.PublishCartCheckout(cart);
        await _cartService.ClearCartAsync(userId);

        return Accepted(new { OrderId = Guid.NewGuid() });

    }
}
