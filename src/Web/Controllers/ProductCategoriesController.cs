using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.ProductCategory.Query.GetCollection;
using Swashbuckle.AspNetCore.Annotations;

namespace Microsoft.Extensions.DependencyInjection.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductCategoriesController: ControllerBase
{
    private readonly IMediator _mediator;

    public ProductCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    //[Authorize/*(Roles = "Admin")*/] // Uncomment if admin-only access is required
    [SwaggerOperation(Summary = "Get all product categories", Description = "Retrieve a list of all product categories.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProductsCategories()
    {
        var categories = await _mediator.Send(new GetProductCategoriesQuery());
        return Ok(categories);
    }
}
