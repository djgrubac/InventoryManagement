namespace Microsoft.Extensions.DependencyInjection.Warehouse.Commands;

public class WarehouseCreateResponse
{
    public Guid Uid { get; set; }
    public string? Name { get; set; }
    public string? Adress { get; set; }
    public string? ContactPerson { get; set; }
    public string? Company { get; set; }
}
