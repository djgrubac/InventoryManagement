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
            Id = Guid.NewGuid(),
            Name = request.Name,
            Address = request.Address,
            ContactPerson = request.ContactPerson,
            Company = request.Company,
            CreatedAt = DateTime.UtcNow
        };
        await _warehouseRepository.AddAsync(warehouse);

        return new WarehouseCreateResponse
        {
            Id = warehouse.Id,
            Name = warehouse.Name,
            Adress = warehouse.Address,
            ContactPerson = warehouse.ContactPerson,
            Company = warehouse.Company,
            CreatedAt = warehouse.CreatedAt
        };
    }
}
