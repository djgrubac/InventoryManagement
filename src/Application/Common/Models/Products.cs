namespace InventoryManagement.Core.DTO;

public class Products
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public int StockQuantity { get; set; }
}
