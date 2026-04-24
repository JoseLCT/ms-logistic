using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.DeliveryZones.GetDeliveryZoneById;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

namespace MsLogistic.Infrastructure.Queries.DeliveryZones;

internal class GetDeliveryZoneByIdHandler : IRequestHandler<GetDeliveryZoneByIdQuery, Result<DeliveryZoneDetailDto>> {
	private readonly PersistenceDbContext _dbContext;

	public GetDeliveryZoneByIdHandler(PersistenceDbContext dbContext) {
		_dbContext = dbContext;
	}

	public async Task<Result<DeliveryZoneDetailDto>> Handle(GetDeliveryZoneByIdQuery request, CancellationToken ct) {
		DeliveryZonePersistenceModel? deliveryZone = await _dbContext.DeliveryZones
			.AsNoTracking()
			.Where(dz => dz.Id == request.Id)
			.FirstOrDefaultAsync(ct);

		if (deliveryZone == null) {
			return Result.Failure<DeliveryZoneDetailDto>(
				CommonErrors.NotFoundById("DeliveryZone", request.Id)
			);
		}

		var dto = new DeliveryZoneDetailDto(
			deliveryZone.Id,
			deliveryZone.DriverId,
			deliveryZone.Code,
			deliveryZone.Name,
			deliveryZone.Boundaries.Coordinates.Select(c => new CoordinateDto(c.Y, c.X)).ToList()
		);

		return Result.Success(dto);
	}
}
