using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagement.Core.Interfaces;

public interface IUserService
{
    Task<Guid> CreateUserAsync(string name, string email, UserRole role);
}