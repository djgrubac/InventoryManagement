using Microsoft.Extensions.DependencyInjection.Product.Events.Create;

namespace Microsoft.Extensions.DependencyInjection.Product.EventHandlers;

public class ProductCreateEventHandler:INotificationHandler<ProductCreateEvent>
{
    public Task Handle(ProductCreateEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
