using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.DeliveryZones.CreateDeliveryZone;
using MsLogistic.Application.DeliveryZones.GetAllDeliveryZones;
using MsLogistic.Application.DeliveryZones.GetDeliveryZoneById;
using MsLogistic.Application.DeliveryZones.RemoveDeliveryZone;
using MsLogistic.Application.DeliveryZones.UpdateDeliveryZone;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.WebApi.Contracts.V1.DeliveryZones;

namespace MsLogistic.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/delivery-zones")]
public class DeliveryZoneController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public DeliveryZoneController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllDeliveryZonesQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetDeliveryZoneByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeliveryZoneContract contract)
    {
        var boundaries = contract.Boundaries
            .Select(c => new CoordinateDto(c.Latitude, c.Longitude))
            .ToList();
        var command = new CreateDeliveryZoneCommand(contract.DriverId, contract.Code, contract.Name, boundaries);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDeliveryZoneContract contract)
    {
        var boundaries = contract.Boundaries
            .Select(c => new CoordinateDto(c.Latitude, c.Longitude))
            .ToList();
        var command = new UpdateDeliveryZoneCommand(id, contract.DriverId, contract.Code, contract.Name, boundaries);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(Guid id)
    {
        var command = new RemoveDeliveryZoneCommand(id);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}