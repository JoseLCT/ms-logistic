using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Batches.Entities;
using MsLogistic.Infrastructure.Persistence.DomainModel;
using MsLogistic.Infrastructure.Persistence.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Persistence.Repositories;

public class BatchRepositoryTest : IDisposable {
    private readonly DomainDbContext _dbContext;
    private readonly BatchRepository _repository;

    public BatchRepositoryTest() {
        var options = new DbContextOptionsBuilder<DomainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new DomainDbContext(options);
        _repository = new BatchRepository(_dbContext);
    }

    public void Dispose() {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenBatchExists_ShouldReturnBatch() {
        // Arrange
        var batch = Batch.Create(10);
        await _dbContext.Batches.AddAsync(batch);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(batch.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(batch.Id);
        result.TotalOrders.Should().Be(10);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBatchDoesNotExist_ShouldReturnNull() {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_WhenBatchesExist_ShouldReturnAllBatches() {
        // Arrange
        var batch1 = Batch.Create(5);
        var batch2 = Batch.Create(10);
        var batch3 = Batch.Create(15);

        await _dbContext.Batches.AddRangeAsync(batch1, batch2, batch3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(b => b.Id == batch1.Id);
        result.Should().Contain(b => b.Id == batch2.Id);
        result.Should().Contain(b => b.Id == batch3.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoBatches_ShouldReturnEmptyList() {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetLatestBatchAsync

    [Fact]
    public async Task GetLatestBatchAsync_ShouldReturnMostRecentBatch() {
        // Arrange
        var batch1 = Batch.Create(5);
        await _dbContext.Batches.AddAsync(batch1);
        await _dbContext.SaveChangesAsync();

        var batch2 = Batch.Create(10);
        await _dbContext.Batches.AddAsync(batch2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestBatchAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(batch2.Id);
    }

    [Fact]
    public async Task GetLatestBatchAsync_WhenNoBatches_ShouldReturnNull() {
        // Act
        var result = await _repository.GetLatestBatchAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldAddBatchToDatabase() {
        // Arrange
        var batch = Batch.Create(10);

        // Act
        await _repository.AddAsync(batch);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedBatch = await _dbContext.Batches.FindAsync(batch.Id);
        savedBatch.Should().NotBeNull();
        savedBatch.Id.Should().Be(batch.Id);
        savedBatch.TotalOrders.Should().Be(10);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ShouldUpdateBatchInDatabase() {
        // Arrange
        var batch = Batch.Create(5);
        await _dbContext.Batches.AddAsync(batch);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(batch).State = EntityState.Detached;

        // Act
        var batchToUpdate = await _dbContext.Batches.FindAsync(batch.Id);
        batchToUpdate!.AddOrders(10);
        _repository.Update(batchToUpdate);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedBatch = await _dbContext.Batches.FindAsync(batch.Id);
        updatedBatch!.TotalOrders.Should().Be(15);
    }

    #endregion

    #region Remove

    [Fact]
    public async Task Remove_ShouldDeleteBatchFromDatabase() {
        // Arrange
        var batch = Batch.Create(10);
        await _dbContext.Batches.AddAsync(batch);
        await _dbContext.SaveChangesAsync();

        // Act
        _repository.Remove(batch);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedBatch = await _dbContext.Batches.FindAsync(batch.Id);
        deletedBatch.Should().BeNull();
    }

    #endregion
}
