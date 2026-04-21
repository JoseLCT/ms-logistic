using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Orders.CreateOrder;
using MsLogistic.Application.Orders.DeliverOrder;
using MsLogistic.Application.Orders.GetAllOrders;
using MsLogistic.Application.Orders.GetOrderById;
using MsLogistic.Application.Orders.ReportIncident;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;
using MsLogistic.WebApi.Contracts.V1.Orders;

namespace MsLogistic.WebApi.Controllers.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/orders")]
public class OrderController : ApiControllerBase {
	private readonly IMediator _mediator;

	public OrderController(IMediator mediator) {
		_mediator = mediator;
	}

	[HttpGet]
	[ProducesResponseType(typeof(Result<IReadOnlyList<OrderSummaryDto>>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetAll() {
		var query = new GetAllOrdersQuery();
		Result<IReadOnlyList<OrderSummaryDto>> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(Result<OrderDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(Result<OrderDetailDto>), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(Guid id) {
		var query = new GetOrderByIdQuery(id);
		Result<OrderDetailDto> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpPost]
	[ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
	public async Task<IActionResult> Create([FromBody] CreateOrderContract contract) {
		var deliveryLocation = new CoordinateDto(
			contract.DeliveryLocation.Latitude,
			contract.DeliveryLocation.Longitude
		);
		var items = contract.Items
			.Select(i => new CreateOrderItemDto(i.ProductId, i.Quantity))
			.ToList();
		var command = new CreateOrderCommand(
			contract.CustomerId,
			contract.ScheduledDeliveryDate,
			contract.DeliveryAddress,
			deliveryLocation,
			items
		);
		Result<Guid> result = await _mediator.Send(command);
		return HandleCreatedResult(
			result,
			nameof(GetById),
			new { id = result.IsSuccess ? result.Value : Guid.Empty }
		);
	}

	[HttpPost("{id:guid}/deliver")]
	[Consumes("multipart/form-data")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Deliver(
		Guid id,
		[FromForm] DeliverOrderContract contract
	) {
		var location = new CoordinateDto(
			contract.Location.Latitude,
			contract.Location.Longitude
		);

		var command = new DeliverOrderCommand {
			OrderId = id,
			DriverId = contract.DriverId,
			Location = location,
			ImageStream = contract.Image.OpenReadStream(),
			ImageFileName = contract.Image.FileName,
			Comments = contract.Comments
		};

		Result result = await _mediator.Send(command);
		return HandleNoContentResult(result);
	}

	[HttpPost("{id:guid}/incident")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> ReportIncident(
		Guid id,
		[FromBody] ReportIncidentContract contract
	) {
		var command = new ReportIncidentCommand {
			OrderId = id,
			DriverId = contract.DriverId,
			IncidentType = contract.IncidentType,
			Description = contract.Description
		};

		Result result = await _mediator.Send(command);
		return HandleNoContentResult(result);
	}
}
