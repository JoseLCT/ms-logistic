using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Drivers.GetAllDrivers;

public record GetAllDriversQuery() : IRequest<Result<IReadOnlyList<DriverSummaryDto>>>;
