using Inventory_Management.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection.ProductCategory.Commands.Create;
using Microsoft.Extensions.DependencyInjection.ProductCategory.Query.GetCollection;
using Entities = Inventory_Management.Domain.Entities;

namespace Inventory_Management.Application.UnitTests.Category;

public class CategoryHandlerTests
{
    private Mock<ICategoryRepository> _categoryRepository = null!;

    [SetUp]
    public void Setup()
    {
        _categoryRepository = new Mock<ICategoryRepository>();
    }

    [Test]
    public async Task Create_AddsCategoryAndReturnsResponse()
    {
        Entities.Category? added = null;
        _categoryRepository.Setup(r => r.AddAsync(It.IsAny<Entities.Category>()))
            .Callback<Entities.Category>(c => added = c);

        var response = await new CreateCategoryCommandHandler(_categoryRepository.Object)
            .Handle(new CreateCategoryCommand { Caption = "Tools" }, CancellationToken.None);

        added.Should().NotBeNull();
        added!.Caption.Should().Be("Tools");
        response.Uid.Should().Be(added.Uid);
        response.Caption.Should().Be("Tools");
    }

    [Test]
    public async Task GetAll_MapsEntitiesToDtos()
    {
        var categories = new[]
        {
            new Entities.Category { Id = 1, Caption = "Tools" },
            new Entities.Category { Id = 2, Caption = "Paint" }
        };
        _categoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var result = await new GetCategoriesQueryHandler(_categoryRepository.Object)
            .Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Select(c => (c.Uid, c.Caption)).Should().Equal(
            (categories[0].Uid, "Tools"),
            (categories[1].Uid, "Paint"));
    }
}
