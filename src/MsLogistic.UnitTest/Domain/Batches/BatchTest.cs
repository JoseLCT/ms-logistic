using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Batches.Entities;
using MsLogistic.Domain.Batches.Enums;
using MsLogistic.Domain.Batches.Errors;
using MsLogistic.Domain.Batches.Events;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Batches;

public class BatchTest {
	#region Create

	[Fact]
	public void Create_WithValidTotalOrders_ShouldSucceed() {
		// Arrange
		int totalOrders = 10;
		DateTime beforeCreation = DateTime.UtcNow;

		// Act
		var batch = Batch.Create(totalOrders);

		// Assert
		batch.Should().NotBeNull();
		batch.Id.Should().NotBe(Guid.Empty);
		batch.TotalOrders.Should().Be(10);
		batch.Status.Should().Be(BatchStatusEnum.Open);
		batch.OpenedAt.Should().BeOnOrAfter(beforeCreation);
		batch.OpenedAt.Should().BeOnOrBefore(DateTime.UtcNow);
		batch.ClosedAt.Should().BeNull();
		batch.UpdatedAt.Should().BeNull();
		batch.DomainEvents.Should().BeEmpty();
	}

	[Fact]
	public void Create_WithoutArguments_ShouldDefaultToZeroOrders() {
		// Act
		var batch = Batch.Create();

		// Assert
		batch.TotalOrders.Should().Be(0);
		batch.Status.Should().Be(BatchStatusEnum.Open);
	}

	[Fact]
	public void Create_WithZeroTotalOrders_ShouldSucceed() {
		// Act
		var batch = Batch.Create();

		// Assert
		batch.TotalOrders.Should().Be(0);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(-100)]
	public void Create_WithNegativeTotalOrders_ShouldThrowDomainException(int totalOrders) {
		// Act
		Action act = () => Batch.Create(totalOrders);

		// Assert
		act.Should().Throw<DomainException>()
			.Which.Error.Should().Be(BatchErrors.TotalOrdersCannotBeNegative);
	}

	#endregion

	#region AddOrders

	[Fact]
	public void AddOrders_WithValidQuantity_ShouldIncreaseTotal() {
		// Arrange
		var batch = Batch.Create(5);

		// Act
		batch.AddOrders(3);

		// Assert
		batch.TotalOrders.Should().Be(8);
	}

	[Fact]
	public void AddOrders_WithValidQuantity_ShouldMarkAsUpdated() {
		// Arrange
		var batch = Batch.Create(5);

		// Act
		batch.AddOrders(3);

		// Assert
		batch.UpdatedAt.Should().NotBeNull();
	}

	[Fact]
	public void AddOrders_MultipleTimes_ShouldAccumulateTotal() {
		// Arrange
		var batch = Batch.Create();

		// Act
		batch.AddOrders(2);
		batch.AddOrders(5);
		batch.AddOrders(3);

		// Assert
		batch.TotalOrders.Should().Be(10);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(-100)]
	public void AddOrders_WithZeroOrNegative_ShouldThrowDomainException(int invalidQuantity) {
		// Arrange
		var batch = Batch.Create();

		// Act
		Action act = () => batch.AddOrders(invalidQuantity);

		// Assert
		act.Should().Throw<DomainException>()
			.Which.Error.Should().Be(BatchErrors.CannotAddNonPositiveQuantityOfOrders);
	}

	[Fact]
	public void AddOrders_WithInvalidQuantity_ShouldNotChangeTotalOrUpdatedAt() {
		// Arrange
		var batch = Batch.Create(5);

		// Act
		Action act = () => batch.AddOrders(0);

		// Assert
		act.Should().Throw<DomainException>();
		batch.TotalOrders.Should().Be(5);
		batch.UpdatedAt.Should().BeNull();
	}

	[Fact]
	public void AddOrders_WhenBatchIsClosed_ShouldThrowDomainException() {
		// Arrange
		var batch = Batch.Create();
		batch.Close();

		// Act
		Action act = () => batch.AddOrders(5);

		// Assert
		act.Should().Throw<DomainException>()
			.Which.Error.Should().Be(BatchErrors.CannotAddOrdersToClosedBatch);
	}

	[Fact]
	public void AddOrders_WhenBatchIsClosed_ShouldNotChangeTotal() {
		// Arrange
		var batch = Batch.Create(5);
		batch.Close();

		// Act
		Action act = () => batch.AddOrders(3);

		// Assert
		act.Should().Throw<DomainException>();
		batch.TotalOrders.Should().Be(5);
	}

	#endregion

	#region Close

	[Fact]
	public void Close_WhenOpen_ShouldUpdateStatusAndClosedAt() {
		// Arrange
		var batch = Batch.Create();
		DateTime beforeClose = DateTime.UtcNow;

		// Act
		batch.Close();

		// Assert
		batch.Status.Should().Be(BatchStatusEnum.Closed);
		batch.ClosedAt.Should().NotBeNull();
		batch.ClosedAt.Should().BeOnOrAfter(beforeClose);
		batch.ClosedAt.Should().BeOnOrBefore(DateTime.UtcNow);
	}

	[Fact]
	public void Close_WhenOpen_ShouldRaiseBatchClosedEventWithCorrectId() {
		// Arrange
		var batch = Batch.Create();

		// Act
		batch.Close();

		// Assert
		batch.DomainEvents.Should().ContainSingle(e => e is BatchClosed);
		BatchClosed closedEvent = batch.DomainEvents.OfType<BatchClosed>().Single();
		closedEvent.BatchId.Should().Be(batch.Id);
	}

	[Fact]
	public void Close_WhenOpen_ShouldMarkAsUpdated() {
		// Arrange
		var batch = Batch.Create();

		// Act
		batch.Close();

		// Assert
		batch.UpdatedAt.Should().NotBeNull();
	}

	[Fact]
	public void Close_WhenAlreadyClosed_ShouldThrowDomainException() {
		// Arrange
		var batch = Batch.Create();
		batch.Close();

		// Act
		Action act = batch.Close;

		// Assert
		act.Should().Throw<DomainException>()
			.Which.Error.Should().Be(BatchErrors.CannotCloseAlreadyClosedBatch);
	}

	[Fact]
	public void Close_WhenAlreadyClosed_ShouldNotRaiseAdditionalEvent() {
		// Arrange
		var batch = Batch.Create();
		batch.Close();

		// Act
		Action act = batch.Close;

		// Assert
		act.Should().Throw<DomainException>();
		batch.DomainEvents.OfType<BatchClosed>().Should().HaveCount(1);
	}

	#endregion
}
