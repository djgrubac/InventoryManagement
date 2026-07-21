namespace Microsoft.Extensions.DependencyInjection.Order.Commands.AddItem;

public class AddOrderItemCommand : IRequest
{
    public Guid OrderUid { get; set; }
    public Guid ProductUid { get; set; }
    public int Quantity { get; set; }

    public AddOrderItemCommand(Guid orderUid, Guid productUid, int quantity)
    {
        OrderUid = orderUid;
        ProductUid = productUid;
        Quantity = quantity;
    }
}

