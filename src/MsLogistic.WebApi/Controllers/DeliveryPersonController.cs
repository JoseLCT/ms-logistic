using MediatR;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.DeliveryPerson.CreateDeliveryPerson;
using MsLogistic.Application.DeliveryPerson.DeleteDeliveryPerson;
using MsLogistic.Application.DeliveryPerson.GetDeliveryPerson;
using MsLogistic.Application.DeliveryPerson.GetDeliveryPersons;
using MsLogistic.Application.DeliveryPerson.UpdateDeliveryPerson;

namespace MsLogistic.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeliveryPersonController : BaseController
{
    private readonly IMediator _mediator;

    public DeliveryPersonController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetDeliveryPersons()
    {
        var query = new GetDeliveryPersonsQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDeliveryPerson(Guid id)
    {
        var query = new GetDeliveryPersonQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDeliveryPerson([FromBody] CreateDeliveryPersonCommand request)
    {
        var result = await _mediator.Send(request);
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDeliveryPerson([FromBody] UpdateDeliveryPersonCommand request)
    {
        var result = await _mediator.Send(request);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDeliveryPerson(Guid id)
    {
        var command = new DeleteDeliveryPersonCommand(id);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}