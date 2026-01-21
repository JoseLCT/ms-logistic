using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Orders.CreateOrder;
using MsLogistic.Application.Orders.GetAllOrders;
using MsLogistic.Application.Orders.GetOrderById;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.WebApi.Contracts.V1.Orders;

namespace MsLogistic.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
public class OrderController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllOrdersQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderContract contract)
    {
        var deliveryLocation = new CoordinateDto(
            contract.DeliveryLocation.Latitude,
            contract.DeliveryLocation.Longitude
        );
        var items = contract.Items
            .Select(i => new CreateOrderItemDto(i.ProductId, i.Quantity))
            .ToList();
        var command = new CreateOrderCommand(
            contract.CustomerId,
            contract.ScheduledDeliveryDate,
            contract.DeliveryAddress,
            deliveryLocation,
            items
        );
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}