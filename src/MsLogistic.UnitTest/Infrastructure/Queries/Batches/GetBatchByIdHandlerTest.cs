using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Batches.GetBatchById;
using MsLogistic.Domain.Batches.Enums;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Batches;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Batches;

public class GetBatchByIdHandlerTest : IDisposable {
    private readonly PersistenceDbContext _dbContext;
    private readonly GetBatchByIdHandler _handler;

    public GetBatchByIdHandlerTest() {
        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PersistenceDbContext(options);
        _handler = new GetBatchByIdHandler(_dbContext);
    }

    public void Dispose() {
        _dbContext.Dispose();
    }

    private static BatchPersistenceModel CreateBatchPersistenceModel(
        Guid? id = null,
        int totalOrders = 0,
        BatchStatusEnum status = BatchStatusEnum.Open,
        DateTime? openedAt = null,
        DateTime? closedAt = null
    ) {
        return new BatchPersistenceModel {
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
    public async Task Handle_WithExistingBatchId_ShouldReturnBatch() {
        // Arrange
        var newBatch = CreateBatchPersistenceModel(totalOrders: 10);

        await _dbContext.Batches.AddAsync(newBatch);
        await _dbContext.SaveChangesAsync();

        var query = new GetBatchByIdQuery(newBatch.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(newBatch.Id);
        result.Value.Status.Should().Be(BatchStatusEnum.Open);
        result.Value.TotalOrders.Should().Be(10);
        result.Value.ClosedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistingBatchId_ShouldReturnNotFoundError() {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var query = new GetBatchByIdQuery(nonExistingId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommonErrors.NotFoundById("Batch", nonExistingId));
    }


    [Fact]
    public async Task Handle_WithMultipleBatches_ShouldReturnCorrectBatch() {
        // Arrange
        var batch1 = CreateBatchPersistenceModel();
        var batch2 = CreateBatchPersistenceModel();

        await _dbContext.Batches.AddRangeAsync(batch1, batch2);
        await _dbContext.SaveChangesAsync();

        var query = new GetBatchByIdQuery(batch2.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(batch2.Id);
    }
}
