namespace Inventory_Management.Domain.Entities;

public class Warehouse : BaseAuditableEntity
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? Company { get; set; }
}
