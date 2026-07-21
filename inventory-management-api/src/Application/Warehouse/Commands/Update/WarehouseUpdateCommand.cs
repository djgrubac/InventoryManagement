namespace Microsoft.Extensions.DependencyInjection.Warehouse.Commands.Update;

public class WarehouseUpdateCommand:IRequest
{
    public Guid Uid { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? Company { get; set; }

    public WarehouseUpdateCommand(Guid uid, string? name, string? address, string? contactPerson, string? company)
    {
        Uid = uid;
        Name = name;
        Address = address;
        ContactPerson = contactPerson;
        Company = company;
    }
}
