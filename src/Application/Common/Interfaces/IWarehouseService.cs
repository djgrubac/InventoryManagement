using Inventory_Management.Application.Common.Models;

namespace Inventory_Management.Application.Common.Interfaces;

public interface IWarehouseService
{
    Task<Guid> CreateWarehouseAsync(string name, string address, string contactPerson, string company);
    Task<IEnumerable<WarehouseDTO>> GetAllWarehousesAsync();
    Task<WarehouseDTO?> GetWarehouseByIdAsync(Guid id);
}
