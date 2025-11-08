using Inventory_Management.Application.Common.Models;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Warehouse.Queries;

public class GetWarehousesQueryHandler:IRequestHandler<GetWarehousesQuery, IEnumerable<WarehouseDTO>>
{
    private readonly IBaseRepository<Entities.Warehouse> _warehouseRepository;

    public GetWarehousesQueryHandler(IBaseRepository<Entities.Warehouse> warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }
    
    public async Task<IEnumerable<WarehouseDTO>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var warehouses = await _warehouseRepository.GetAllAsync();
        return warehouses.Select(warehouses => new WarehouseDTO
        {
            Id = warehouses.Id,
            Name = warehouses.Name,
            Address = warehouses.Address,
            ContactPerson = warehouses.ContactPerson
        });
    }
}
