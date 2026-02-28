using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Drivers.CreateDriver;
using MsLogistic.Application.Drivers.GetAllDrivers;
using MsLogistic.Application.Drivers.GetDriverById;
using MsLogistic.Application.Drivers.RemoveDriver;
using MsLogistic.Application.Drivers.UpdateDriver;
using MsLogistic.WebApi.Contracts.V1.Drivers;

namespace MsLogistic.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/drivers")]
public class DriverController : ApiControllerBase {
    private readonly IMediator _mediator;

    public DriverController(IMediator mediator) {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() {
        var query = new GetAllDriversQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) {
        var query = new GetDriverByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDriverContract contract) {
        var command = new CreateDriverCommand(contract.FullName);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDriverContract contract) {
        var command = new UpdateDriverCommand(id, contract.FullName, contract.IsActive);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(Guid id) {
        var command = new RemoveDriverCommand(id);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}
