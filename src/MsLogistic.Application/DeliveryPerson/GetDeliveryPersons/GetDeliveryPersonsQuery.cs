using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryPerson.GetDeliveryPersons;

public record GetDeliveryPersonsQuery() : IRequest<Result<ICollection<DeliveryPersonSummaryDto>>>;