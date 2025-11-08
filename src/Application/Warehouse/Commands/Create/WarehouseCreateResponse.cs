namespace Microsoft.Extensions.DependencyInjection.Warehouse.Commands;

public class WarehouseCreateResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Adress { get; set; }
    public string? ContactPerson { get; set; }
    public string? Company { get; set; }
    public DateTime CreatedAt { get; set; }
}
