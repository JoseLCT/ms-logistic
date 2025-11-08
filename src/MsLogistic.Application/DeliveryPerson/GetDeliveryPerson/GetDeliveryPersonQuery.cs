using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryPerson.GetDeliveryPerson;

public record GetDeliveryPersonQuery(Guid Id) : IRequest<Result<DeliveryPersonDetailDto>>;