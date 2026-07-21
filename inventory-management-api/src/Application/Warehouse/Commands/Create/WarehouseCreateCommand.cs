namespace Microsoft.Extensions.DependencyInjection.Warehouse.Commands;

public class WarehouseCreateCommand:IRequest<WarehouseCreateResponse>
{
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string ContactPerson { get; set; }
    public required string Company { get; set; }

    public WarehouseCreateCommand(string name, string address, string contactPerson, string company)
    {
        Name = name;
        Address = address;
        ContactPerson = contactPerson;
        Company = company;
    }
}
