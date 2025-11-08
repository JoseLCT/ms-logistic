using MediatR;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Order.CreateOrder;
using MsLogistic.Application.Order.GetOrder;
using MsLogistic.Application.Order.GetOrders;

namespace MsLogistic.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : BaseController
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var query = new GetOrdersQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var query = new GetOrderQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand request)
    {
        var result = await _mediator.Send(request);
        return HandleResult(result);
    }
}