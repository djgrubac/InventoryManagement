using InventoryManagement.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.Product.Commands.Delete;
using Entities = Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.UnitTests.Product.Commands;

public class ProductDeleteCommandHandlerTests
{
    private Mock<IBaseRepository<Entities.Product>> _productRepository = null!;
    private ProductDeleteCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _productRepository = new Mock<IBaseRepository<Entities.Product>>();
        _handler = new ProductDeleteCommandHandler(_productRepository.Object);
    }

    [Test]
    public async Task Handle_ProductExists_DeletesIt()
    {
        var product = new Entities.Product { Id = 1 };
        _productRepository.Setup(r => r.GetByUidAsync(product.Uid)).ReturnsAsync(product);

        await _handler.Handle(new ProductDeleteCommand(product.Uid), CancellationToken.None);

        _productRepository.Verify(r => r.DeleteAsync(product.Uid), Times.Once);
    }

    [Test]
    public async Task Handle_ProductMissing_ThrowsKeyNotFoundExceptionAndDoesNotDelete()
    {
        var act = () => _handler.Handle(new ProductDeleteCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _productRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
