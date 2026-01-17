using Inventory_Management.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.WarehouseStock.Commands;
using Microsoft.Extensions.DependencyInjection.WarehouseStock.Queries;
using Microsoft.Extensions.DependencyInjection.WarehouseStock.Queries.GetCollection;
using Swashbuckle.AspNetCore.Annotations;


namespace Microsoft.Extensions.DependencyInjection.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StocksController : ControllerBase
{
    private readonly IMediator _mediator;

    public StocksController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("{warehouseUid:guid}/{productUid:guid}")]    
    [SwaggerOperation(Summary = "Get stock by Uid", Description = "Retrieve the details of a specific stock by Uid.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StockDTO>> GetStockByUid(Guid warehouseUid, Guid productUid)
    {
        var result = await _mediator.Send(new GetStockByUidQuery(warehouseUid, productUid));

        if (result == null)
            return NotFound();

        return Ok(result);
    }
    
    [HttpGet]    
    [SwaggerOperation(Summary = "Get all stocks", Description = "Retrieve a list of all stocks.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetsStocksResult>> Get([FromQuery] GetStocksQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    // PUT /api/stocks/adjust
    [HttpPut("adjust")]
    // [Authorize/*(Roles = "Admin")*/] // Uncomment if admin-only access is required
    [SwaggerOperation(Summary = "Adjust quatity", Description = "Adjust quatity of a stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AdjustStockQuantity([FromBody] AdjustStockCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}
