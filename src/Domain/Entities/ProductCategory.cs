namespace Inventory_Management.Domain.Entities;

public class ProductCategory
{
    public Guid Id { get; set; }
    public required string Caption { get; set; }
}
