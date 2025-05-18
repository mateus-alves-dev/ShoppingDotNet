namespace Cart.Application.DTOs;

public class CartDto
{
    public Guid UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}
