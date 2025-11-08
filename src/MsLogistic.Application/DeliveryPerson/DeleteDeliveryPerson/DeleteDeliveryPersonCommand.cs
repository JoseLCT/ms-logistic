using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryPerson.DeleteDeliveryPerson;

public record DeleteDeliveryPersonCommand(Guid Id) : IRequest<Result<Guid>>;