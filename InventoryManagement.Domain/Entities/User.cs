using InventoryManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagement.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string Name { get; set; } //IdentityUser already has id and email
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
