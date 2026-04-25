using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Batches.CloseCurrentBatch;
using MsLogistic.Application.Customers.CreateCustomer;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Batches.Entities;
using MsLogistic.Domain.Batches.Enums;
using MsLogistic.Domain.Batches.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Application.Batches;

public class CloseCurrentBatchHandlerTest {
	private readonly Mock<IBatchRepository> _batchRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly CloseCurrentBatchHandler _handler;

	public CloseCurrentBatchHandlerTest() {
		_batchRepositoryMock = new Mock<IBatchRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<CreateCustomerHandler>>();

		_handler = new CloseCurrentBatchHandler(
			_batchRepositoryMock.Object,
			_unitOfWorkMock.Object,
			loggerMock.Object
		);
	}

	[Fact]
	public async Task Handle_WhenOpenBatchExists_ShouldCloseAndCommit() {
		// Arrange
		var batch = Batch.Create(10);

		_batchRepositoryMock
			.Setup(r => r.GetLatestBatchAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(batch);

		var command = new CloseCurrentBatchCommand();

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		batch.Status.Should().Be(BatchStatusEnum.Closed);

		_batchRepositoryMock.Verify(r => r.Update(batch), Times.Once);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenNoBatchExists_ShouldReturnFailure() {
		// Arrange
		_batchRepositoryMock
			.Setup(r => r.GetLatestBatchAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync((Batch?)null);

		var command = new CloseCurrentBatchCommand();

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CloseCurrentBatchErrors.NoOpenBatchAvailable);

		_batchRepositoryMock.Verify(r => r.Update(It.IsAny<Batch>()), Times.Never);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenLatestBatchIsAlreadyClosed_ShouldReturnFailure() {
		// Arrange
		var batch = Batch.Create(10);
		batch.Close();

		_batchRepositoryMock
			.Setup(r => r.GetLatestBatchAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(batch);

		var command = new CloseCurrentBatchCommand();

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CloseCurrentBatchErrors.LatestBatchAlreadyClosed);

		_batchRepositoryMock.Verify(r => r.Update(It.IsAny<Batch>()), Times.Never);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}
