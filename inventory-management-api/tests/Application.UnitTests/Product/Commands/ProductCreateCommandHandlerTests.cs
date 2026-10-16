using Inventory_Management.Application.Common.Interfaces;
using InventoryManagement.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection.Product.Commands.Create;
using Entities = Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.UnitTests.Product.Commands;

public class ProductCreateCommandHandlerTests
{
    private Mock<IBaseRepository<Entities.Product>> _productRepository = null!;
    private Mock<ICategoryRepository> _categoryRepository = null!;
    private ProductCreateCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _productRepository = new Mock<IBaseRepository<Entities.Product>>();
        _categoryRepository = new Mock<ICategoryRepository>();
        _handler = new ProductCreateCommandHandler(_productRepository.Object, Mock.Of<IMediator>(), _categoryRepository.Object);
    }

    [Test]
    public async Task Handle_CategoryExists_AddsProductAndReturnsResponse()
    {
        var category = new Entities.Category { Id = 3, Caption = "Tools" };
        _categoryRepository.Setup(r => r.GetByUidAsync(category.Uid)).ReturnsAsync(category);
        Entities.Product? added = null;
        _productRepository.Setup(r => r.AddAsync(It.IsAny<Entities.Product>()))
            .Callback<Entities.Product>(p => added = p);

        var response = await _handler.Handle(
            new ProductCreateCommand("Hammer", category.Uid, 12.5m, 4, "Steel"), CancellationToken.None);

        added.Should().NotBeNull();
        added!.Name.Should().Be("Hammer");
        added.Price.Should().Be(12.5m);
        added.StockQuantity.Should().Be(4);
        added.Description.Should().Be("Steel");
        added.CategoryId.Should().Be(category.Id);
        response.Uid.Should().Be(added.Uid);
        response.Name.Should().Be("Hammer");
    }

    [Test]
    public async Task Handle_CategoryMissing_ThrowsAndDoesNotAddProduct()
    {
        var act = () => _handler.Handle(
            new ProductCreateCommand("Hammer", Guid.NewGuid(), 1m, 1, null), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
        _productRepository.Verify(r => r.AddAsync(It.IsAny<Entities.Product>()), Times.Never);
    }
}
