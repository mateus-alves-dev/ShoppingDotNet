namespace Cart.Domain.Entities;

public class Cart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public decimal TotalAmount { get; set; }

    public Cart()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice, string productName, string productImageUrl)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            // Update details in case they changed in ProductService
            existingItem.UnitPrice = unitPrice; 
            existingItem.ProductName = productName;
            existingItem.ProductImageUrl = productImageUrl;
            UpdatedAt = DateTime.UtcNow; // Ensure UpdatedAt is set
            return; // Return after updating existing item
        }
        Items.Add(new CartItem
        {
            Id = Guid.NewGuid(), // Set new Guid for the cart item
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            ProductName = productName,
            ProductImageUrl = productImageUrl
        });
        UpdatedAt = DateTime.UtcNow; // Ensure UpdatedAt is set
    }

    public void RemoveItem(Guid itemId)
    {
        Items.RemoveAll(i => i.Id == itemId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        Items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
