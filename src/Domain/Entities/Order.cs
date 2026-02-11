namespace Inventory_Management.Domain.Entities;

public class Order : BaseAuditableEntity
{
    public OrderStatus Status { get; set; }
    public OrderType Type { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}
