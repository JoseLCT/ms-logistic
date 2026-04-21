using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.DeliveryZones.CreateDeliveryZone;
using MsLogistic.Application.DeliveryZones.GetAllDeliveryZones;
using MsLogistic.Application.DeliveryZones.GetDeliveryZoneById;
using MsLogistic.Application.DeliveryZones.RemoveDeliveryZone;
using MsLogistic.Application.DeliveryZones.UpdateDeliveryZone;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;
using MsLogistic.WebApi.Contracts.V1.DeliveryZones;

namespace MsLogistic.WebApi.Controllers.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/delivery-zones")]
public class DeliveryZoneController : ApiControllerBase {
	private readonly IMediator _mediator;

	public DeliveryZoneController(IMediator mediator) {
		_mediator = mediator;
	}

	[HttpGet]
	[ProducesResponseType(typeof(Result<IReadOnlyList<DeliveryZoneSummaryDto>>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetAll() {
		var query = new GetAllDeliveryZonesQuery();
		Result<IReadOnlyList<DeliveryZoneSummaryDto>> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(Result<DeliveryZoneDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(Result<DeliveryZoneDetailDto>), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(Guid id) {
		var query = new GetDeliveryZoneByIdQuery(id);
		Result<DeliveryZoneDetailDto> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpPost]
	[ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
	public async Task<IActionResult> Create([FromBody] CreateDeliveryZoneContract contract) {
		var boundaries = contract.Boundaries
			.Select(c => new CoordinateDto(c.Latitude, c.Longitude))
			.ToList();
		var command = new CreateDeliveryZoneCommand(contract.DriverId, contract.Code, contract.Name, boundaries);
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
	public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDeliveryZoneContract contract) {
		var boundaries = contract.Boundaries
			.Select(c => new CoordinateDto(c.Latitude, c.Longitude))
			.ToList();
		var command = new UpdateDeliveryZoneCommand(id, contract.DriverId, contract.Code, contract.Name, boundaries);
		Result result = await _mediator.Send(command);
		return HandleNoContentResult(result);
	}

	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Remove(Guid id) {
		var command = new RemoveDeliveryZoneCommand(id);
		Result result = await _mediator.Send(command);
		return HandleNoContentResult(result);
	}
}
