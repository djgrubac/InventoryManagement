namespace Inventory_Management.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public required string? Caption { get; set; }
}
