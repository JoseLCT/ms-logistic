using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Batches.GetBatchById;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Batches;

internal class GetBatchByIdHandler : IRequestHandler<GetBatchByIdQuery, Result<BatchDetailDto>> {
    private readonly PersistenceDbContext _dbContext;

    public GetBatchByIdHandler(PersistenceDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<Result<BatchDetailDto>> Handle(GetBatchByIdQuery request, CancellationToken ct) {
        var batch = await _dbContext.Batches
            .AsNoTracking()
            .Where(b => b.Id == request.Id)
            .Select(b => new BatchDetailDto(
                b.Id,
                b.TotalOrders,
                b.Status,
                b.OpenedAt,
                b.ClosedAt
            ))
            .FirstOrDefaultAsync(ct);

        if (batch == null) {
            return Result.Failure<BatchDetailDto>(
                CommonErrors.NotFoundById("Batch", request.Id)
            );
        }

        return Result.Success(batch);
    }
}
