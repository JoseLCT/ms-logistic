using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Drivers.CreateDriver;

public record CreateDriverCommand(string FullName) : IRequest<Result<Guid>>;
