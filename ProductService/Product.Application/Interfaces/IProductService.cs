using Product.Application.DTOs;

namespace Product.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(CreateProductDto dto);
    Task UpdateAsync(Guid id, UpdateProductDto dto);
    Task DeleteAsync(Guid id);
}
