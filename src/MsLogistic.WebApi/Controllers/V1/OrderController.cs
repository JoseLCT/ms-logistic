using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Orders.CreateOrder;
using MsLogistic.Application.Orders.DeliverOrder;
using MsLogistic.Application.Orders.GetAllOrders;
using MsLogistic.Application.Orders.GetOrderById;
using MsLogistic.Application.Orders.ReportIncident;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.WebApi.Contracts.V1.Orders;

namespace MsLogistic.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
public class OrderController : ApiControllerBase {
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator) {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() {
        var query = new GetAllOrdersQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
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
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/deliver")]
    public async Task<IActionResult> Deliver(
        Guid id,
        [FromForm] DeliverOrderContract contract,
        [FromForm] IFormFile image
    ) {
        var location = new CoordinateDto(
            contract.Location.Latitude,
            contract.Location.Longitude
        );

        var command = new DeliverOrderCommand {
            OrderId = id,
            DriverId = contract.DriverId,
            Location = location,
            ImageStream = image.OpenReadStream(),
            ImageFileName = image.FileName,
            Comments = contract.Comments
        };

        var result = await _mediator.Send(command);

        return HandleResult(result);
    }

    [HttpPost("{id:guid}/incident")]
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

        var result = await _mediator.Send(command);

        return HandleResult(result);
    }
}
