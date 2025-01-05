using InventoryManagement.Core.DTO;

namespace InventoryManagement.Core.Interfaces;

public interface IBaseRepository<T> where T: class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
