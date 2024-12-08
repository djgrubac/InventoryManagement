using InventoryManagement.Core.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;

namespace InventoryManagement.Core.Users;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;

    public UserRepository(DataContext context)
    {
        _context = context;
    }

    public Task<User> GetByIdAsync(Guid id)
    {
        return _context.Users.FindAsync(id);
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        return _userRepositoryImplementation.GetAllAsync();
    }

    public Task AddAsync(User entity)
    {
        return _userRepositoryImplementation.AddAsync(entity);
    }

    public Task UpdateAsync(User entity)
    {
        return _userRepositoryImplementation.UpdateAsync(entity);
    }

    public Task DeleteAsync(Guid id)
    {
        return _userRepositoryImplementation.DeleteAsync(id);
    }
}
