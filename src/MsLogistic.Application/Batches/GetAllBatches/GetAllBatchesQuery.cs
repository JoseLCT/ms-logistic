using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Batches.GetAllBatches;

public record GetAllBatchesQuery() : IRequest<Result<IReadOnlyList<BatchSummaryDto>>>;
