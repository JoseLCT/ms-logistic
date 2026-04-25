using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Application.Orders.CreateOrder;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Routes.Entities;
using MsLogistic.Domain.Routes.Repositories;
using MsLogistic.Domain.Shared.Errors;

namespace MsLogistic.Application.Routes.StartRoute;

public class StartRouteHandler : IRequestHandler<StartRouteCommand, Result> {
	private readonly IRouteRepository _routeRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<CreateOrderHandler> _logger;

	public StartRouteHandler(
		IRouteRepository routeRepository,
		IUnitOfWork unitOfWork,
		ILogger<CreateOrderHandler> logger
	) {
		_routeRepository = routeRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<Result> Handle(StartRouteCommand request, CancellationToken ct) {
		Route? route = await _routeRepository.GetByIdAsync(request.Id, ct);

		if (route is null) {
			return Result.Failure(
				CommonErrors.NotFoundById("Route", request.Id)
			);
		}

		route.Start();

		_routeRepository.Update(route);
		await _unitOfWork.CommitAsync(ct);

		return Result.Success();
	}
}
