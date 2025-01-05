namespace Inventory_Management.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } // Primary Key
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
