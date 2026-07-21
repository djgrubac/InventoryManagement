
namespace InventoryManagement.Core.Interfaces;

public interface IBaseRepository<T> where T: class
{
    Task<T?> GetByUidAsync(Guid uid);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
