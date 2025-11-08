using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Warehouse.Commands;
using Microsoft.Extensions.DependencyInjection.Warehouse.Commands.Update;
using Microsoft.Extensions.DependencyInjection.Warehouse.Queries;
using Microsoft.Extensions.DependencyInjection.Warehouse.Queries.GetSingle;
using Swashbuckle.AspNetCore.Annotations;

namespace Microsoft.Extensions.DependencyInjection.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WarehousesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarehousesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    //[Authorize/*(Roles = "Admin")*/] // Uncomment if admin-only access is required
    [SwaggerOperation(Summary = "Get all warehouses", Description = "Retrieve a list of all warehouses.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWarehouses()
    {
        var warehouses = await _mediator.Send(new GetWarehousesQuery());
        return Ok(warehouses);
    }

    [HttpGet("{id}")]
    // [Authorize/*(Roles = "Admin")*/] // Uncomment if admin-only access is required
    [SwaggerOperation(Summary = "Get warehouse by Id",
        Description = "Retrieve the details of a specific warehouse by Id.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWarehouseById([FromRoute] Guid id)
    {
        var warehouse = await _mediator.Send(new GetWarehouseByIdQuery(id));
        return warehouse == null ? NotFound() : Ok(warehouse);
    }

    [HttpPost]
    // [Authorize/*(Roles = "Admin")*/] // Uncomment if admin-only access is required
    [SwaggerOperation(Summary = "Add warehouse", Description = "Create a new warehouse.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] WarehouseCreateCommand command)
    {
        var warehouse = await _mediator.Send(command);
        return Ok(warehouse);
    }

    [HttpPut("{id:guid}")]
    // [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Update warehouse", Description = "Update an existing warehouse by Id.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] WarehouseUpdateCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("The product Id in the URL does not match the Id in the body.");
        }
        
        await _mediator.Send(command);
        return NoContent();
    }
}
