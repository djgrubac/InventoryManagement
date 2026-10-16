using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.Product.Commands.Delete;
using ValidationException = Inventory_Management.Application.Common.Exceptions.ValidationException;
using Entities = Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.UnitTests.Product.Commands;

public class ProductDeleteCommandHandlerTests
{
    private Mock<IBaseRepository<Entities.Product>> _productRepository = null!;
    private Mock<IOrderItemRepository> _orderItemRepository = null!;
    private Mock<IStockRepository> _stockRepository = null!;
    private ProductDeleteCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _productRepository = new Mock<IBaseRepository<Entities.Product>>();
        _orderItemRepository = new Mock<IOrderItemRepository>();
        _stockRepository = new Mock<IStockRepository>();
        _handler = new ProductDeleteCommandHandler(
            _productRepository.Object,
            _orderItemRepository.Object,
            _stockRepository.Object);
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

    [Test]
    public async Task Handle_ProductOnAnOrder_ThrowsValidationExceptionAndDoesNotDelete()
    {
        var product = new Entities.Product { Id = 1 };
        _productRepository.Setup(r => r.GetByUidAsync(product.Uid)).ReturnsAsync(product);
        _orderItemRepository.Setup(r => r.AnyForProductAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _handler.Handle(new ProductDeleteCommand(product.Uid), CancellationToken.None);

        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().ContainKey(nameof(ProductDeleteCommand.Uid));
        _productRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Handle_ProductWithStockOnHand_ThrowsValidationExceptionAndDoesNotDelete()
    {
        var product = new Entities.Product { Id = 1 };
        _productRepository.Setup(r => r.GetByUidAsync(product.Uid)).ReturnsAsync(product);
        _stockRepository.Setup(r => r.AnyOnHandForProductAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _handler.Handle(new ProductDeleteCommand(product.Uid), CancellationToken.None);

        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().ContainKey(nameof(ProductDeleteCommand.Uid));
        _productRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
