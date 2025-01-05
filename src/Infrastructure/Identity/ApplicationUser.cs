// using InventoryManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Inventory_Management.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? Name { get; set; } // Full name of the user
    // public UserRole Role { get; set; } // Enum for roles (Admin, Client, etc.)
    public DateTime CreatedAt { get; set; } // When the user was created
}
