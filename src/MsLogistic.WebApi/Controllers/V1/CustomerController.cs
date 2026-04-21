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

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/customers")]
public class CustomerController : ApiControllerBase {
	private readonly IMediator _mediator;

	public CustomerController(IMediator mediator) {
		_mediator = mediator;
	}

	[HttpGet]
	[ProducesResponseType(typeof(Result<IReadOnlyList<CustomerSummaryDto>>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetAll() {
		var query = new GetAllCustomersQuery();
		Result<IReadOnlyList<CustomerSummaryDto>> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(Result<CustomerDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(Result<CustomerDetailDto>), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(Guid id) {
		var query = new GetCustomerByIdQuery(id);
		Result<CustomerDetailDto> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpPost]
	[ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
	public async Task<IActionResult> Create([FromBody] CreateCustomerContract contract) {
		var command = new CreateCustomerCommand(contract.FullName, contract.PhoneNumber);
		Result<Guid> result = await _mediator.Send(command);
		return HandleCreatedResult(
			result,
			nameof(GetById),
			new { id = result.IsSuccess ? result.Value : Guid.Empty }
		);
	}

	[HttpPut("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerContract contract) {
		var command = new UpdateCustomerCommand(id, contract.FullName, contract.PhoneNumber);
		Result result = await _mediator.Send(command);
		return HandleNoContentResult(result);
	}

	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Remove(Guid id) {
		var command = new RemoveCustomerCommand(id);
		Result result = await _mediator.Send(command);
		return HandleNoContentResult(result);
	}
}
