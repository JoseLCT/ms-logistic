using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Batches.Entities;
using MsLogistic.Domain.Batches.Repositories;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class BatchRepository : IBatchRepository
{
    private readonly DomainDbContext _dbContext;

    public BatchRepository(DomainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Batch>> GetAllAsync(CancellationToken ct = default)
    {
        var batches = await _dbContext.Batches.ToListAsync(ct);
        return batches;
    }

    public async Task<Batch?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var batch = await _dbContext.Batches.FirstOrDefaultAsync(b => b.Id == id, ct);
        return batch;
    }
    
    public async Task<Batch?> GetLatestBatchAsync(CancellationToken ct = default)
    {
        var batch = await _dbContext.Batches
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(ct);
        return batch;
    }

    public async Task AddAsync(Batch batch, CancellationToken ct = default)
    {
        await _dbContext.Batches.AddAsync(batch, ct);
    }

    public void Update(Batch batch)
    {
        _dbContext.Batches.Update(batch);
    }

    public void Remove(Batch batch)
    {
        _dbContext.Batches.Remove(batch);
    }
}