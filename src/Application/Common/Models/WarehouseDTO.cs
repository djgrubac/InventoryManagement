namespace Inventory_Management.Application.Common.Models;

public class WarehouseDTO
{
    public Guid Id { get; set; }
    public string? Name { get; set; } = default!;
    public string? Address { get; set; } = default!;
    public string? ContactPerson { get; set; }
}
