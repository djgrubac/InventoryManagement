using ValidationException = Inventory_Management.Application.Common.Exceptions.ValidationException;
using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Product.Commands.Delete;

public class ProductDeleteCommandHandler:IRequestHandler<ProductDeleteCommand>
{
    private readonly IBaseRepository<Entities.Product> _productRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IStockRepository _stockRepository;

    public ProductDeleteCommandHandler(
        IBaseRepository<Entities.Product> productRepository,
        IOrderItemRepository orderItemRepository,
        IStockRepository stockRepository)
    {
        _productRepository = productRepository;
        _orderItemRepository = orderItemRepository;
        _stockRepository = stockRepository;
    }

    public async Task Handle(ProductDeleteCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByUidAsync(request.Uid);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with Uid {request.Uid} not found.");
        }

        // Order lines are history and the FK is Restrict, so refuse up front instead of surfacing a DbUpdateException
        if (await _orderItemRepository.AnyForProductAsync(product.Id, cancellationToken))
        {
            throw new ValidationException(nameof(request.Uid), "Product cannot be deleted because it appears on one or more orders.");
        }

        // Empty stock rows cascade with the product, but stock on hand must be cleared first
        if (await _stockRepository.AnyOnHandForProductAsync(product.Id, cancellationToken))
        {
            throw new ValidationException(nameof(request.Uid), "Product cannot be deleted while a warehouse still holds stock of it.");
        }

        await _productRepository.DeleteAsync(request.Uid);
    }
}
