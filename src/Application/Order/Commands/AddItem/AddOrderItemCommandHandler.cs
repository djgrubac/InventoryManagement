using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.Interfaces;
using Entities = Inventory_Management.Domain.Entities;

namespace Microsoft.Extensions.DependencyInjection.Order.Commands.AddItem;

public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBaseRepository<Entities.Product> _productRepository;

    public AddOrderItemCommandHandler(
        IOrderRepository orderRepository,
        IBaseRepository<Entities.Product> productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        // Validate order exists
        var order = await _orderRepository.GetByUidAsync(request.OrderUid);
        if (order == null)
        {
            throw new ArgumentException($"Order with UID {request.OrderUid} not found.");
        }

        // Validate product exists
        var product = await _productRepository.GetByUidAsync(request.ProductUid);
        if (product == null)
        {
            throw new ArgumentException($"Product with UID {request.ProductUid} not found.");
        }

        // Validate quantity
        if (request.Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.");
        }

        // Check if product already exists in order items
        var existingItem = order.Items.FirstOrDefault(i => i.Product.Uid == request.ProductUid);
        
        if (existingItem != null)
        {
            // Update existing item quantity
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            // Add new order item
            var orderItem = new Entities.OrderItem
            {
                Product = product,
                Quantity = request.Quantity,
                Price = product.Price // Using current product price
            };
            
            order.Items.Add(orderItem);
        }

        // Recalculate total price
        order.TotalPrice = order.Items.Sum(i => i.Price * i.Quantity);

        await _orderRepository.UpdateAsync(order);
    }
}

