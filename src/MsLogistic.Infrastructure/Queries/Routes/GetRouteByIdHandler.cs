using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Routes.GetRouteById;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Routes;

internal class GetRouteByIdHandler : IRequestHandler<GetRouteByIdQuery, Result<RouteDetailDto>> {
	private readonly PersistenceDbContext _dbContext;

	public GetRouteByIdHandler(PersistenceDbContext dbContext) {
		_dbContext = dbContext;
	}

	public async Task<Result<RouteDetailDto>> Handle(GetRouteByIdQuery request, CancellationToken ct) {
		var result = await _dbContext.Routes
			.AsNoTracking()
			.Where(r => r.Id == request.Id)
			.Select(r => new {
				r.Id,
				r.BatchId,
				r.DeliveryZoneId,
				r.DriverId,
				r.OriginLocation,
				r.Status,
				r.StartedAt,
				r.CompletedAt,
				Stops = _dbContext.Orders
					.Where(o => o.RouteId == r.Id)
					.OrderBy(o => o.DeliverySequence)
					.Select(o => new {
						o.Id,
						o.DeliverySequence,
						o.Status,
						o.DeliveryLocation
					})
					.ToList()
			})
			.FirstOrDefaultAsync(ct);

		if (result is null) {
			return Result.Failure<RouteDetailDto>(
				CommonErrors.NotFoundById("Route", request.Id)
			);
		}

		var dto = new RouteDetailDto(
			result.Id,
			result.BatchId,
			result.DeliveryZoneId,
			result.DriverId,
			new CoordinateDto(
				Latitude: result.OriginLocation.Y,
				Longitude: result.OriginLocation.X
			),
			result.Status,
			result.StartedAt,
			result.CompletedAt,
			result.Stops.Select(s => new RouteStopDto(
				s.Id,
				s.DeliverySequence,
				s.Status,
				new CoordinateDto(
					Latitude: s.DeliveryLocation.Y,
					Longitude: s.DeliveryLocation.X
				)
			)).ToList()
		);

		return Result.Success(dto);
	}
}
