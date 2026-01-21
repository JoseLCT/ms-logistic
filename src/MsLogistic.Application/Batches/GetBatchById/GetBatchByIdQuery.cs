using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Batches.GetBatchById;

public record GetBatchByIdQuery(Guid Id) : IRequest<Result<BatchDetailDto>>;