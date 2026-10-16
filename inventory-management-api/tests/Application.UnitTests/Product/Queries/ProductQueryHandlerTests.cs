using InventoryManagement.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.Product.Queries.GetCollection;
using Microsoft.Extensions.DependencyInjection.Product.Queries.GetSingle;
using Entities = Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.UnitTests.Product.Queries;

public class ProductQueryHandlerTests
{
    private Mock<IBaseRepository<Entities.Product>> _productRepository = null!;

    [SetUp]
    public void Setup()
    {
        _productRepository = new Mock<IBaseRepository<Entities.Product>>();
    }

    [Test]
    public async Task GetProducts_MapsEntitiesToDtos()
    {
        var product = CreateProduct();
        _productRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { product });

        var result = await new GetProductsQueryHandler(_productRepository.Object)
            .Handle(new GetProductsQuery(), CancellationToken.None);

        var dto = result.Should().ContainSingle().Subject;
        dto.Uid.Should().Be(product.Uid);
        dto.Name.Should().Be(product.Name);
        dto.CategoryUid.Should().Be(product.Category.Uid);
        dto.Price.Should().Be(product.Price);
        dto.StockQuantity.Should().Be(product.StockQuantity);
        dto.Description.Should().Be(product.Description);
    }

    [Test]
    public async Task GetProductByUid_ProductExists_ReturnsDto()
    {
        var product = CreateProduct();
        _productRepository.Setup(r => r.GetByUidAsync(product.Uid)).ReturnsAsync(product);

        var dto = await new GetProductByUidQueryHandler(_productRepository.Object)
            .Handle(new GetProductByUidQuery(product.Uid), CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Uid.Should().Be(product.Uid);
        dto.CategoryUid.Should().Be(product.Category.Uid);
    }

    [Test]
    public async Task GetProductByUid_ProductMissing_ReturnsNull()
    {
        var dto = await new GetProductByUidQueryHandler(_productRepository.Object)
            .Handle(new GetProductByUidQuery(Guid.NewGuid()), CancellationToken.None);

        dto.Should().BeNull();
    }

    private static Entities.Product CreateProduct() => new()
    {
        Id = 1,
        Name = "Hammer",
        Price = 12.5m,
        StockQuantity = 3,
        Description = "Steel",
        Category = new Entities.Category { Id = 2, Caption = "Tools" }
    };
}
