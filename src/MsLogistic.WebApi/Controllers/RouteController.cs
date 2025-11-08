using MediatR;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Order.CreateOrder;
using MsLogistic.Application.Order.GetOrder;
using MsLogistic.Application.Order.GetOrders;
using MsLogistic.Application.Route.GetRoutes;

namespace MsLogistic.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RouteController : BaseController
{
    private readonly IMediator _mediator;

    public RouteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoutes()
    {
        var query = new GetRoutesQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}