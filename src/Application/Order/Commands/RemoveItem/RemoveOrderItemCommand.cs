namespace Microsoft.Extensions.DependencyInjection.Order.Commands.RemoveItem;

public class RemoveOrderItemCommand : IRequest
{
    public Guid OrderUid { get; set; }
    public Guid ProductUid { get; set; }

    public RemoveOrderItemCommand(Guid orderUid, Guid productUid)
    {
        OrderUid = orderUid;
        ProductUid = productUid;
    }
}

