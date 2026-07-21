namespace Inventory_Management.Domain.Entities;

public class Category : BaseAuditableEntity
{
    public required string? Caption { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
