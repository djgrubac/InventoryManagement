using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Warehouse.Commands.Update;

public class WarehouseUpdateCommandHandler: IRequestHandler<WarehouseUpdateCommand>
{
    private readonly IBaseRepository<Entities.Warehouse> _warehouseRepository;

    public WarehouseUpdateCommandHandler(IBaseRepository<Entities.Warehouse> warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task Handle(WarehouseUpdateCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseRepository.GetByUidAsync(request.Uid);
        if (warehouse == null)
        {
            throw new KeyNotFoundException($"Warehouse with Uid {request.Uid} not found.");
        }
        
        warehouse.Name = request.Name;
        warehouse.Address = request.Address;
        warehouse.ContactPerson = request.ContactPerson;
        warehouse.Company = request.Company;
        warehouse.LastModified = DateTimeOffset.UtcNow;
        
        await _warehouseRepository.UpdateAsync(warehouse);
    }
}
