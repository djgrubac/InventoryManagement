using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Warehouse.Commands;

public class WarehouseCreateCommandHandler:IRequestHandler<WarehouseCreateCommand, WarehouseCreateResponse>
{
    private readonly IBaseRepository<Entities.Warehouse> _warehouseRepository;

    public WarehouseCreateCommandHandler(IBaseRepository<Entities.Warehouse> warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }
    
    public async Task<WarehouseCreateResponse> Handle(WarehouseCreateCommand request, CancellationToken cancellationToken)
    {
        var warehouse = new Entities.Warehouse
        {
            Name = request.Name,
            Address = request.Address,
            ContactPerson = request.ContactPerson,
            Company = request.Company
        };
        await _warehouseRepository.AddAsync(warehouse);

        return new WarehouseCreateResponse
        {
            Uid = warehouse.Uid,
            Name = warehouse.Name,
            Adress = warehouse.Address,
            ContactPerson = warehouse.ContactPerson,
            Company = warehouse.Company
        };
    }
}
