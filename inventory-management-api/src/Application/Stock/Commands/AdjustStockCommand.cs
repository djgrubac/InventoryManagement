using Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.WarehouseStock.Commands;

public record AdjustStockCommand(
    Guid WarehouseUid,
    Guid ProductUid,
    int NewQuantity
) : IRequest;
