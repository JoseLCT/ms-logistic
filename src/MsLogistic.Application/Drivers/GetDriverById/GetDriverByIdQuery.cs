using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Drivers.GetDriverById;

public record GetDriverByIdQuery(Guid Id) : IRequest<Result<DriverDetailDto>>;