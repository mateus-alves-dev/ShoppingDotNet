using Cart.Application.DTOs;

namespace Cart.Application.Interfaces;

public interface IProductServiceHttpClient
{
    Task<ProductDto?> GetProductByIdAsync(Guid productId);
}
