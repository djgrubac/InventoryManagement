using System.Linq.Expressions;

namespace Inventory_Management.Application.Common.Models;

public class WarehouseDTO
{
    public Guid Uid { get; set; }
    public string? Name { get; set; } = default!;
    public string? Address { get; set; } = default!;
    public string? ContactPerson { get; set; }
    
    public static Expression<Func<Domain.Entities.Warehouse, WarehouseDTO>> Projection
    {
        get
        {
            return entity => new WarehouseDTO
            {
                Uid = entity.Uid,
                Name = entity.Name,
                Address = entity.Address,
                ContactPerson = entity.ContactPerson
            };
        }
    }
}
