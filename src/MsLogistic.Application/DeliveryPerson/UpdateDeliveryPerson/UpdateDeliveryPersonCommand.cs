using MediatR;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryPerson.Types;

namespace MsLogistic.Application.DeliveryPerson.UpdateDeliveryPerson;

public record UpdateDeliveryPersonCommand(
    Guid Id,
    string Name,
    bool IsActive,
    DeliveryPersonStatusType Status
) : IRequest<Result<Guid>>;