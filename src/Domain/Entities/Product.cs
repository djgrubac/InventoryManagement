using Microsoft.AspNetCore.Identity;

namespace Inventory_Management.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } // Primary Key
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public Guid ProductCategoryId { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
