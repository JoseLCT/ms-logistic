using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Batches.GetAllBatches;
using MsLogistic.Domain.Batches.Enums;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Batches;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Batches;

public class GetAllBatchesHandlerTest : IDisposable
{
    private readonly PersistenceDbContext _dbContext;
    private readonly GetAllBatchesHandler _handler;

    public GetAllBatchesHandlerTest()
    {
        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PersistenceDbContext(options);
        _handler = new GetAllBatchesHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private static BatchPersistenceModel CreateBatchPersistenceModel(
        Guid? id = null,
        int totalOrders = 0,
        BatchStatusEnum status = BatchStatusEnum.Open,
        DateTime? openedAt = null,
        DateTime? closedAt = null
    )
    {
        return new BatchPersistenceModel
        {
            Id = id ?? Guid.NewGuid(),
            TotalOrders = totalOrders,
            Status = status,
            OpenedAt = openedAt ?? DateTime.UtcNow,
            ClosedAt = closedAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    [Fact]
    public async Task Handle_WithNoBatches_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllBatchesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSingleBatch_ShouldReturnListWithOneBatch()
    {
        // Arrange
        var batch = CreateBatchPersistenceModel();

        await _dbContext.Batches.AddAsync(batch);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllBatchesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(batch.Id);
        result.Value[0].Status.Should().Be(BatchStatusEnum.Open);
        result.Value[0].ClosedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithMultipleBatches_ShouldReturnAllBatches()
    {
        // Arrange
        var batch1 = CreateBatchPersistenceModel();
        var batch2 = CreateBatchPersistenceModel();

        await _dbContext.Batches.AddRangeAsync(batch1, batch2);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllBatchesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}