namespace InventoryManagement.Core.DTO;

public class ProductsDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public int StockQuantity { get; set; }
}