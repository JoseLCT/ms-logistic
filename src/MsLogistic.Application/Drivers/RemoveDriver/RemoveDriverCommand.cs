using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Drivers.RemoveDriver;

public record RemoveDriverCommand(Guid Id) : IRequest<Result<Guid>>;