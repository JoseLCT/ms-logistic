using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Batches.GetAllBatches;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Batches.Enums;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Batches;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Batches;

public class GetAllBatchesHandlerTest : IDisposable {
	private readonly PersistenceDbContext _dbContext;
	private readonly GetAllBatchesHandler _handler;

	public GetAllBatchesHandlerTest() {
		DbContextOptions<PersistenceDbContext> options = new DbContextOptionsBuilder<PersistenceDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		_dbContext = new PersistenceDbContext(options);
		_handler = new GetAllBatchesHandler(_dbContext);
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
	public async Task Handle_WithNoBatches_ShouldReturnEmptyList() {
		// Arrange
		var query = new GetAllBatchesQuery();

		// Act
		Result<IReadOnlyList<BatchSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithSingleBatch_ShouldReturnListWithOneBatch() {
		// Arrange
		BatchPersistenceModel batch = CreateBatchPersistenceModel();

		await _dbContext.Batches.AddAsync(batch);
		await _dbContext.SaveChangesAsync();

		var query = new GetAllBatchesQuery();

		// Act
		Result<IReadOnlyList<BatchSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(1);
		result.Value[0].Id.Should().Be(batch.Id);
		result.Value[0].Status.Should().Be(BatchStatusEnum.Open);
		result.Value[0].OpenedAt.Should().Be(batch.OpenedAt);
		result.Value[0].ClosedAt.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithClosedBatch_ShouldMapClosedAtCorrectly() {
		// Arrange
		DateTime openedAt = DateTime.UtcNow.AddHours(-2);
		DateTime closedAt = DateTime.UtcNow;
		BatchPersistenceModel batch = CreateBatchPersistenceModel(
			status: BatchStatusEnum.Closed,
			openedAt: openedAt,
			closedAt: closedAt
		);

		await _dbContext.Batches.AddAsync(batch);
		await _dbContext.SaveChangesAsync();

		var query = new GetAllBatchesQuery();

		// Act
		Result<IReadOnlyList<BatchSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(1);
		result.Value[0].Id.Should().Be(batch.Id);
		result.Value[0].Status.Should().Be(BatchStatusEnum.Closed);
		result.Value[0].OpenedAt.Should().Be(openedAt);
		result.Value[0].ClosedAt.Should().Be(closedAt);
	}

	[Fact]
	public async Task Handle_WithMultipleBatches_ShouldReturnAllBatches() {
		// Arrange
		BatchPersistenceModel batch1 = CreateBatchPersistenceModel();
		BatchPersistenceModel batch2 = CreateBatchPersistenceModel();
		BatchPersistenceModel batch3 = CreateBatchPersistenceModel(
			status: BatchStatusEnum.Closed,
			closedAt: DateTime.UtcNow
		);

		await _dbContext.Batches.AddRangeAsync(batch1, batch2, batch3);
		await _dbContext.SaveChangesAsync();

		var query = new GetAllBatchesQuery();

		// Act
		Result<IReadOnlyList<BatchSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(3);
		result.Value.Select(b => b.Id).Should()
			.BeEquivalentTo([batch1.Id, batch2.Id, batch3.Id]);
	}
}
