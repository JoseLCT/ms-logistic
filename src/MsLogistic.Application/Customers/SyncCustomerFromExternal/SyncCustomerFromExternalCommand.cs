using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Customers.SyncCustomerFromExternal;

public record SyncCustomerFromExternalCommand(Guid ExternalId, string FullName, string? PhoneNumber)
	: IRequest<Result<Guid>>;
