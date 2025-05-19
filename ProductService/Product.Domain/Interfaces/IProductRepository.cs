namespace Product.Domain.Interfaces;


public interface IProductRepository
{
    Task<IEnumerable<Entities.Product>> GetAllAsync();
    Task<Entities.Product?> GetByIdAsync(Guid id);
    Task AddAsync(Entities.Product product);
    Task DeleteAsync(Guid id);
    Task UpdateAsync(Entities.Product product);
}
