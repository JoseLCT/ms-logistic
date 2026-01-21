using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Customers.CreateCustomer;
using MsLogistic.Application.Customers.GetAllCustomers;
using MsLogistic.Application.Customers.GetCustomerById;
using MsLogistic.Application.Customers.RemoveCustomer;
using MsLogistic.Application.Customers.UpdateCustomer;
using MsLogistic.WebApi.Contracts.V1.Customers;

namespace MsLogistic.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customers")]
public class CustomerController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllCustomersQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerContract contract)
    {
        var command = new CreateCustomerCommand(contract.FullName, contract.PhoneNumber);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerContract contract)
    {
        var command = new UpdateCustomerCommand(id, contract.FullName, contract.PhoneNumber);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(Guid id)
    {
        var command = new RemoveCustomerCommand(id);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}