using InventoryManagement.Core.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagement.Core.Users;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task<Guid> CreateUserAsync(string name, string email, UserRole role)
    {
        var user = new User
        {
            UserName = email,
            Email = email,
            Name = name,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, "DefaultPassword123!"); // Replace with user-specified password if required

        if (!result.Succeeded)
        {
            throw new Exception("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return user.Id;
    }
}