using Inventory_Management.Application.Common.Models;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Warehouse.Queries.GetSingle;

public class GetWarehouseByIdQueryHandler:IRequestHandler<GetWarehouseByIdQuery,WarehouseDTO?>
{
    private readonly IBaseRepository<Entities.Warehouse> _warehouseRepository;

    public GetWarehouseByIdQueryHandler(IBaseRepository<Entities.Warehouse> warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }
    
    public async Task<WarehouseDTO?> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseRepository.GetByUidAsync(request.Uid);
        if (warehouse == null)
            return null;
        
        return new WarehouseDTO
        {
            Uid = warehouse.Uid,
            Name = warehouse.Name,
            Address = warehouse.Address,
            ContactPerson = warehouse.ContactPerson,
        };
    }
}
