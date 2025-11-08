using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Application.Common.Models;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Warehouse;

public class WarehouseService: IWarehouseService
{
    private readonly IBaseRepository<Entities.Warehouse> _warehouseRepository;

    public WarehouseService(IBaseRepository<Entities.Warehouse> warehouseRepository, IMapper mapper)
    {
        _warehouseRepository = warehouseRepository;
    }
    public async Task<Guid> CreateWarehouseAsync(string name, string address, string contactPerson, string company)
    {
        var warehouse = new Entities.Warehouse
        {
            Id = Guid.NewGuid(),
            Name = name,
            Address = address,
            ContactPerson = contactPerson,
            Company = company,
            CreatedAt = DateTime.UtcNow
        };
        
        await _warehouseRepository.AddAsync(warehouse);
        return warehouse.Id;
    }

    public async Task<IEnumerable<WarehouseDTO>> GetAllWarehousesAsync()
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

    public async Task<WarehouseDTO?> GetWarehouseByIdAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null)
            return null;
        return new WarehouseDTO
        {
            Id = warehouse.Id,
            Name = warehouse.Name,
            Address = warehouse.Address,
            ContactPerson = warehouse.ContactPerson
        };
    }
}
