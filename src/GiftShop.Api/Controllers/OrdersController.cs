using GiftShop.Api.Dtos;
using GiftShop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiftShop.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        var order = await _orders.CreateOrderAsync(request);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> Get(int id)
        => Ok(await _orders.GetOrderAsync(id));

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<OrderResponse>> ChangeStatus(int id, UpdateStatusRequest request)
        => Ok(await _orders.ChangeStatusAsync(id, request.Status));

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<OrderResponse>> Cancel(int id)
        => Ok(await _orders.CancelOrderAsync(id));

    [HttpPut("{id:int}/delivery")]
    public async Task<ActionResult<OrderResponse>> UpdateDelivery(int id, UpdateDeliveryRequest request)
        => Ok(await _orders.UpdateDeliveryAsync(id, request));

    [HttpPost("{id:int}/items")]
    public async Task<ActionResult<OrderResponse>> AddItem(int id, AddItemRequest request)
        => Ok(await _orders.AddItemAsync(id, request));

    [HttpDelete("{id:int}/items/{itemId:int}")]
    public async Task<ActionResult<OrderResponse>> RemoveItem(int id, int itemId)
        => Ok(await _orders.RemoveItemAsync(id, itemId));
}
