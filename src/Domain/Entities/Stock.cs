namespace Inventory_Management.Domain.Entities;

public class Stock : BaseAuditableEntity
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public Product Product { get; set; } = default!;
    public Warehouse Warehouse { get; set; } = default!;
    public int Quantity { get; set; }
}
