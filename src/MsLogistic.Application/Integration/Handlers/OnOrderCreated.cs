using Joselct.Communication.Contracts.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Application.Integration.Events.Incoming;
using MsLogistic.Application.Orders.CreateOrder;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Repositories;

namespace MsLogistic.Application.Integration.Handlers;

public class OnOrderCreated : IIntegrationMessageConsumer<OrderCreatedMessage> {
	private readonly IMediator _mediator;
	private readonly ICustomerRepository _customerRepository;
	private readonly IProductRepository _productRepository;
	private readonly ILogger<OnOrderCreated> _logger;

	public OnOrderCreated(
		IMediator mediator,
		ICustomerRepository customerRepository,
		IProductRepository productRepository,
		ILogger<OnOrderCreated> logger
	) {
		_mediator = mediator;
		_customerRepository = customerRepository;
		_productRepository = productRepository;
		_logger = logger;
	}

	public async Task HandleAsync(OrderCreatedMessage message, CancellationToken ct = default) {
		Customer? customer = await _customerRepository
			.GetByExternalIdAsync(message.CustomerId, ct);

		if (customer == null) {
			_logger.LogWarning(
				"Customer with external ID {CustomerId} not found. Skipping order creation.",
				message.CustomerId
			);
			return;
		}

		var externalProductIds = message.Items.Select(i => i.RecipeId).Distinct().ToList();
		IReadOnlyList<Product> products = await _productRepository
			.GetByExternalIdsAsync(externalProductIds, ct);

		var productsByExternalId = products.ToDictionary(p => p.ExternalId!.Value, p => p);

		var missingProducts = externalProductIds
			.Where(id => !productsByExternalId.ContainsKey(id))
			.ToList();

		if (missingProducts.Count > 0) {
			_logger.LogWarning(
				"Products with external IDs {ProductIds} not found. Skipping order creation.",
				string.Join(", ", missingProducts)
			);
			return;
		}

		var items = message.Items
			.Select(i => new CreateOrderItemDto(
				ProductId: productsByExternalId[i.RecipeId].Id,
				Quantity: i.Quantity
			))
			.ToList();

		var command = new CreateOrderCommand(
			CustomerId: customer.Id,
			ScheduledDeliveryDate: message.DeliveryDate,
			DeliveryAddress: message.DeliveryAddress,
			DeliveryLocation: new CoordinateDto(
				Latitude: message.DeliveryLocation.Latitude,
				Longitude: message.DeliveryLocation.Longitude
			),
			Items: items
		);

		await _mediator.Send(command, ct);
	}
}
