using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Products.SyncProductFromExternal;

public record SyncProductFromExternalCommand(Guid ExternalId, string Name, string? Description = null)
	: IRequest<Result<Guid>>;
