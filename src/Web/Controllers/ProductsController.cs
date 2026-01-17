using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Product.Commands.Create;
using Microsoft.Extensions.DependencyInjection.Product.Commands.Delete;
using Microsoft.Extensions.DependencyInjection.Product.Commands.Update;
using Microsoft.Extensions.DependencyInjection.Product.Queries.GetCollection;
using Microsoft.Extensions.DependencyInjection.Product.Queries.GetSingle;
using Swashbuckle.AspNetCore.Annotations;

namespace Microsoft.Extensions.DependencyInjection.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ProductsController: ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    //[Authorize/*(Roles = "Admin")*/] // Uncomment if admin-only access is required
    [SwaggerOperation(Summary = "Get all products", Description = "Retrieve a list of all products.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _mediator.Send(new GetProductsQuery());
        return Ok(products);
    }

    [HttpGet("{uid}")]
    // [Authorize/*(Roles = "Admin")*/] // Uncomment if admin-only access is required
    [SwaggerOperation(Summary = "Get product by Uid", Description = "Retrieve the details of a specific product by Uid.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProductById(Guid uid)
    {
        var product = await _mediator.Send(new GetProductByUidQuery(uid));
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost]
    // [Authorize/*(Roles = "Admin")*/] // Uncomment if admin-only access is required
    [SwaggerOperation(Summary = "Add product", Description = "Create a new product.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] ProductCreateCommand command)
    {
        var product = await _mediator.Send(command);
        return Ok(product);
    }

    [HttpPut("{uid:guid}")]
    // [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Update product", Description = "Update an existing product by Uid.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid uid, [FromBody] ProductUpdateCommand command)
    {
        if (uid != command.Uid)
        {
            return BadRequest("The product Uid in the URL does not match the Uid in the body.");
        }
        
        await _mediator.Send(command);
        
        return NoContent();
    }
    
    [HttpDelete]
    // [Authorize/*(Roles = "Admin")*/] // Uncomment if admin-only access is required
    [SwaggerOperation(Summary = "Remove product", Description = "Delete a product.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid uid)
    {
        var command = new ProductDeleteCommand(uid);
        
        await _mediator.Send(command);
        
        return NoContent();
    }

}
