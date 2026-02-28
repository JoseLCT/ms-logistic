using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Drivers.UpdateDriver;

public record UpdateDriverCommand(Guid Id, string FullName, bool IsActive) : IRequest<Result<Guid>>;
