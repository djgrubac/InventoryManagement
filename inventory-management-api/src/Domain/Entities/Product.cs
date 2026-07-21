using Microsoft.AspNetCore.Identity;

namespace Inventory_Management.Domain.Entities;

public class Product : BaseAuditableEntity
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public int StockQuantity { get; set; }
    public string? Description { get; set; }
    public string? UserId { get; set; } //public string? Manufacturer
}
