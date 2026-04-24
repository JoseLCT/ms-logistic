using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Batches.CloseCurrentBatch;

public record CloseCurrentBatchCommand() : IRequest<Result>;
