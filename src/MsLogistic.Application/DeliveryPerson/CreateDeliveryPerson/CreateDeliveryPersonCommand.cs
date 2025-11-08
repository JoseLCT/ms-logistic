using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryPerson.CreateDeliveryPerson;

public record CreateDeliveryPersonCommand(string Name) : IRequest<Result<Guid>>;