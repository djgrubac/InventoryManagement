using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.Product.Commands.Update;
using Entities = Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.UnitTests.Product.Commands;

public class ProductUpdateCommandHandlerTests
{
    private Mock<IBaseRepository<Entities.Product>> _productRepository = null!;
    private Mock<ICategoryRepository> _categoryRepository = null!;
    private ProductUpdateCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _productRepository = new Mock<IBaseRepository<Entities.Product>>();
        _categoryRepository = new Mock<ICategoryRepository>();
        _handler = new ProductUpdateCommandHandler(_productRepository.Object, _categoryRepository.Object);
    }

    [Test]
    public async Task Handle_ProductAndCategoryExist_UpdatesAllFields()
    {
        var product = new Entities.Product { Id = 1, Name = "Old", Price = 1m, StockQuantity = 1, Description = "old" };
        var category = new Entities.Category { Id = 9, Caption = "New category" };
        _productRepository.Setup(r => r.GetByUidAsync(product.Uid)).ReturnsAsync(product);
        _categoryRepository.Setup(r => r.GetByUidAsync(category.Uid)).ReturnsAsync(category);

        await _handler.Handle(
            new ProductUpdateCommand(product.Uid, "New", category.Uid, 20m, 7, "new"), CancellationToken.None);

        product.Name.Should().Be("New");
        product.Price.Should().Be(20m);
        product.StockQuantity.Should().Be(7);
        product.Description.Should().Be("new");
        product.Category.Should().BeSameAs(category);
        product.CategoryId.Should().Be(9);
        _productRepository.Verify(r => r.UpdateAsync(product), Times.Once);
    }

    [Test]
    public async Task Handle_ProductMissing_ThrowsKeyNotFoundException()
    {
        var act = () => _handler.Handle(
            new ProductUpdateCommand(Guid.NewGuid(), "New", Guid.NewGuid(), 1m, 1, ""), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Test]
    public async Task Handle_CategoryMissing_ThrowsKeyNotFoundException()
    {
        var product = new Entities.Product { Id = 1, Name = "Old" };
        _productRepository.Setup(r => r.GetByUidAsync(product.Uid)).ReturnsAsync(product);

        var act = () => _handler.Handle(
            new ProductUpdateCommand(product.Uid, "New", Guid.NewGuid(), 1m, 1, ""), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        product.Name.Should().Be("Old");
        _productRepository.Verify(r => r.UpdateAsync(It.IsAny<Entities.Product>()), Times.Never);
    }
}
