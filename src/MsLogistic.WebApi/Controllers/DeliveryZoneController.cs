using MediatR;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.DeliveryZone.CreateDeliveryZone;
using MsLogistic.Application.DeliveryZone.DeleteDeliveryZone;
using MsLogistic.Application.DeliveryZone.GetDeliveryZone;
using MsLogistic.Application.DeliveryZone.GetDeliveryZones;
using MsLogistic.Application.DeliveryZone.UpdateDeliveryZone;

namespace MsLogistic.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeliveryZoneController : BaseController
{
    private readonly IMediator _mediator;

    public DeliveryZoneController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetDeliveryZones()
    {
        var query = new GetDeliveryZonesQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDeliveryZone(Guid id)
    {
        var query = new GetDeliveryZoneQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDeliveryZone([FromBody] CreateDeliveryZoneCommand request)
    {
        var result = await _mediator.Send(request);
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDeliveryZone([FromBody] UpdateDeliveryZoneCommand request)
    {
        var result = await _mediator.Send(request);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDeliveryZone(Guid id)
    {
        var command = new DeleteDeliveryZoneCommand(id);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}