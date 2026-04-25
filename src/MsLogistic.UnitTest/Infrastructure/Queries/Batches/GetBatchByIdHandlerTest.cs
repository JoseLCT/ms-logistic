using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Batches.GetBatchById;
using MsLogistic.Core.Results;
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
		DbContextOptions<PersistenceDbContext> options = new DbContextOptionsBuilder<PersistenceDbContext>()
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
		BatchPersistenceModel newBatch = CreateBatchPersistenceModel(totalOrders: 10);

		await _dbContext.Batches.AddAsync(newBatch);
		await _dbContext.SaveChangesAsync();

		var query = new GetBatchByIdQuery(newBatch.Id);

		// Act
		Result<BatchDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(newBatch.Id);
		result.Value.TotalOrders.Should().Be(10);
		result.Value.Status.Should().Be(BatchStatusEnum.Open);
		result.Value.OpenedAt.Should().Be(newBatch.OpenedAt);
		result.Value.ClosedAt.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithClosedBatch_ShouldMapAllFieldsCorrectly() {
		// Arrange
		DateTime openedAt = DateTime.UtcNow.AddHours(-5);
		DateTime closedAt = DateTime.UtcNow;
		BatchPersistenceModel newBatch = CreateBatchPersistenceModel(
			totalOrders: 25,
			status: BatchStatusEnum.Closed,
			openedAt: openedAt,
			closedAt: closedAt
		);

		await _dbContext.Batches.AddAsync(newBatch);
		await _dbContext.SaveChangesAsync();

		var query = new GetBatchByIdQuery(newBatch.Id);

		// Act
		Result<BatchDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(newBatch.Id);
		result.Value.TotalOrders.Should().Be(25);
		result.Value.Status.Should().Be(BatchStatusEnum.Closed);
		result.Value.OpenedAt.Should().Be(openedAt);
		result.Value.ClosedAt.Should().Be(closedAt);
	}

	[Fact]
	public async Task Handle_WithNonExistingBatchId_ShouldReturnNotFoundError() {
		// Arrange
		var nonExistingId = Guid.NewGuid();
		var query = new GetBatchByIdQuery(nonExistingId);

		// Act
		Result<BatchDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Batch", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithNonExistingBatchIdAndOtherBatchesInDb_ShouldReturnNotFoundError() {
		// Arrange
		BatchPersistenceModel batch1 = CreateBatchPersistenceModel();
		BatchPersistenceModel batch2 = CreateBatchPersistenceModel();

		await _dbContext.Batches.AddRangeAsync(batch1, batch2);
		await _dbContext.SaveChangesAsync();

		var nonExistingId = Guid.NewGuid();
		var query = new GetBatchByIdQuery(nonExistingId);

		// Act
		Result<BatchDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Batch", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithMultipleBatches_ShouldReturnCorrectBatch() {
		// Arrange
		BatchPersistenceModel batch1 = CreateBatchPersistenceModel(totalOrders: 5);
		BatchPersistenceModel batch2 = CreateBatchPersistenceModel(totalOrders: 15);
		BatchPersistenceModel batch3 = CreateBatchPersistenceModel(totalOrders: 30);

		await _dbContext.Batches.AddRangeAsync(batch1, batch2, batch3);
		await _dbContext.SaveChangesAsync();

		var query = new GetBatchByIdQuery(batch2.Id);

		// Act
		Result<BatchDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(batch2.Id);
		result.Value.TotalOrders.Should().Be(15);
		result.Value.OpenedAt.Should().Be(batch2.OpenedAt);
	}
}
