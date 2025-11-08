using MediatR;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Customer.CreateCustomer;
using MsLogistic.Application.Customer.DeleteCustomer;
using MsLogistic.Application.Customer.GetCustomer;
using MsLogistic.Application.Customer.GetCustomers;
using MsLogistic.Application.Customer.UpdateCustomer;

namespace MsLogistic.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerController : BaseController
{
    private readonly IMediator _mediator;

    public CustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var query = new GetCustomersQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomer(Guid id)
    {
        var query = new GetCustomerQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand request)
    {
        var result = await _mediator.Send(request);
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerCommand request)
    {
        var result = await _mediator.Send(request);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        var command = new DeleteCustomerCommand(id);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}