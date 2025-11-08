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
        var warehouse = await _warehouseRepository.GetByIdAsync(request.Id);
        if (warehouse == null)
            return null;
        
        return new WarehouseDTO
        {
            Id = warehouse.Id,
            Name = warehouse.Name,
            Address = warehouse.Address,
            ContactPerson = warehouse.ContactPerson,
        };
    }
}
