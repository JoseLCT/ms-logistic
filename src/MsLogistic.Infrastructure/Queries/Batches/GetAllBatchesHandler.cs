using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Batches.GetAllBatches;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Batches;

internal class GetAllBatchesHandler : IRequestHandler<GetAllBatchesQuery, Result<IReadOnlyList<BatchSummaryDto>>> {
    private readonly PersistenceDbContext _dbContext;

    public GetAllBatchesHandler(PersistenceDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<BatchSummaryDto>>> Handle(GetAllBatchesQuery request, CancellationToken ct) {
        var batches = await _dbContext.Batches
            .AsNoTracking()
            .Select(b => new BatchSummaryDto(
                b.Id,
                b.Status,
                b.OpenedAt,
                b.ClosedAt
            ))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<BatchSummaryDto>>(batches);
    }
}
