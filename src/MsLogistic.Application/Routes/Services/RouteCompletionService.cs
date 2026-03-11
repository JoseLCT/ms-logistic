using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Domain.Routes.Entities;
using MsLogistic.Domain.Routes.Repositories;

namespace MsLogistic.Application.Routes.Services;

public class RouteCompletionService {
	private readonly IOrderRepository _orderRepository;
	private readonly IRouteRepository _routeRepository;

	private static readonly OrderStatusEnum[] TerminalStatuses =
	[
		OrderStatusEnum.Delivered,
		OrderStatusEnum.Failed,
		OrderStatusEnum.Cancelled
	];

	public RouteCompletionService(
		IOrderRepository orderRepository,
		IRouteRepository routeRepository
	) {
		_orderRepository = orderRepository;
		_routeRepository = routeRepository;
	}

	public async Task<bool> TryCompleteRouteAsync(Guid? routeId, CancellationToken ct) {
		if (routeId is null)
			return false;

		Route? route = await _routeRepository.GetByIdAsync(routeId.Value, ct);
		if (route is null)
			return false;

		IReadOnlyList<Order> orders = await _orderRepository.GetByRouteIdAsync(routeId.Value, ct);
		if (orders.Count == 0)
			return false;

		bool allTerminal = orders.All(o => TerminalStatuses.Contains(o.Status));
		if (!allTerminal)
			return false;

		route.Complete();
		_routeRepository.Update(route);
		return true;
	}
}
