using Inventory_Management.Application.Common.Models;
using Inventory_Management.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Order.Commands.AddItem;
using Microsoft.Extensions.DependencyInjection.Order.Commands.ChangeStatus;
using Microsoft.Extensions.DependencyInjection.Order.Commands.Create;
using Microsoft.Extensions.DependencyInjection.Order.Commands.RemoveItem;
using Microsoft.Extensions.DependencyInjection.Order.Queries.GetCollection;
using Microsoft.Extensions.DependencyInjection.Order.Queries.GetSingle;

namespace Inventory_Management.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateCommand command)
    {
        var orderUid = await _mediator.Send(command);
        
        return CreatedAtAction(
            nameof(GetOrderByUid),
            new { uid = orderUid },
            orderUid
        );
    }

    [HttpGet("{uid}")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderByUid(Guid uid)
    {
        var query = new GetOrderByUidQuery(uid);
        var order = await _mediator.Send(query);
        
        if (order == null)
            return NotFound();
        
        return Ok(order);
    }
    
    [HttpGet("warehouse/{warehouseUid}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByWarehouse(Guid warehouseUid)
    {
        var query = new GetOrdersByWarehouseQuery(warehouseUid);
        var orders = await _mediator.Send(query);
        
        return Ok(orders);
    }
    
    [HttpGet("{orderUid}/items")]
    [ProducesResponseType(typeof(IEnumerable<OrderItemDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderItems(Guid orderUid)
    {
        var query = new GetOrderItemsByOrderUidQuery(orderUid);
        var items = await _mediator.Send(query);
        
        return Ok(items);
    }

    [HttpPost("{orderUid}/items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddOrderItem(Guid orderUid, [FromBody] AddOrderItemCommand command)
    {
        if (orderUid != command.OrderUid)
            return BadRequest("Order UID mismatch between route and body");
        
        await _mediator.Send(command);
        
        return NoContent();
    }
    
    [HttpDelete("{orderUid}/items/{productUid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveOrderItem(Guid orderUid, Guid productUid)
    {
        var command = new RemoveOrderItemCommand(orderUid, productUid);
        await _mediator.Send(command);
        
        return NoContent();
    }
    
    [HttpPatch("{orderUid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeOrderStatus(Guid orderUid, [FromBody] ChangeOrderStatusCommand command)
    {
        if (orderUid != command.OrderUid)
            return BadRequest("Order UID mismatch between route and body");
        
        await _mediator.Send(command);
        
        return NoContent();
    }
}
