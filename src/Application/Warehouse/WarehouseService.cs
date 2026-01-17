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
            Uid = Guid.NewGuid(),
            Name = name,
            Address = address,
            ContactPerson = contactPerson,
            Company = company,
        };
        
        await _warehouseRepository.AddAsync(warehouse);
        return warehouse.Uid;
    }

    public async Task<IEnumerable<WarehouseDTO>> GetAllWarehousesAsync()
    {
        var warehouses = await _warehouseRepository.GetAllAsync();
        return warehouses.Select(warehouses => new WarehouseDTO
        {
            Uid = warehouses.Uid,
            Name = warehouses.Name,
            Address = warehouses.Address,
            ContactPerson = warehouses.ContactPerson
        });
    }

    public async Task<WarehouseDTO?> GetWarehouseByIdAsync(Guid uid)
    {
        var warehouse = await _warehouseRepository.GetByUidAsync(uid);
        if (warehouse == null)
            return null;
        return new WarehouseDTO
        {
            Uid = warehouse.Uid,
            Name = warehouse.Name,
            Address = warehouse.Address,
            ContactPerson = warehouse.ContactPerson
        };
    }
}
