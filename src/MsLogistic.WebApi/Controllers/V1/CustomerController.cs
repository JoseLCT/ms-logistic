using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Customers.CreateCustomer;
using MsLogistic.Application.Customers.GetAllCustomers;
using MsLogistic.Application.Customers.GetCustomerById;
using MsLogistic.Application.Customers.RemoveCustomer;
using MsLogistic.Application.Customers.UpdateCustomer;
using MsLogistic.Core.Results;
using MsLogistic.WebApi.Contracts.V1.Customers;

namespace MsLogistic.WebApi.Controllers.V1;

// [Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/customers")]
public class CustomerController : ApiControllerBase {
	private readonly IMediator _mediator;

	public CustomerController(IMediator mediator) {
		_mediator = mediator;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll() {
		var query = new GetAllCustomersQuery();
		Result<IReadOnlyList<CustomerSummaryDto>> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpGet("{id:guid}")]
	public async Task<IActionResult> GetById(Guid id) {
		var query = new GetCustomerByIdQuery(id);
		Result<CustomerDetailDto> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateCustomerContract contract) {
		var command = new CreateCustomerCommand(contract.FullName, contract.PhoneNumber);
		Result<Guid> result = await _mediator.Send(command);
		return HandleResult(result);
	}

	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerContract contract) {
		var command = new UpdateCustomerCommand(id, contract.FullName, contract.PhoneNumber);
		Result<Guid> result = await _mediator.Send(command);
		return HandleResult(result);
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Remove(Guid id) {
		var command = new RemoveCustomerCommand(id);
		Result<Guid> result = await _mediator.Send(command);
		return HandleResult(result);
	}
}
