using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Batches.Entities;
using MsLogistic.Domain.Batches.Enums;
using MsLogistic.Domain.Batches.Errors;
using MsLogistic.Domain.Batches.Events;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Batches;

public class BatchTest
{
    #region Create

    [Fact]
    public void Create_WithValidTotalOrders_ShouldSucceed()
    {
        // Arrange
        var totalOrders = 10;

        // Act
        var batch = Batch.Create(totalOrders);

        // Assert
        batch.Should().NotBeNull();
        batch.TotalOrders.Should().Be(10);
        batch.Status.Should().Be(BatchStatusEnum.Open);
        batch.ClosedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithNegativeTotalOrders_ShouldThrowDomainException()
    {
        // Arrange
        var totalOrders = -5;

        // Act
        Action act = () => Batch.Create(totalOrders);

        // Assert
        act.Should().Throw<DomainException>().Which.Error.Should().Be(BatchErrors.TotalOrdersCannotBeNegative);
    }

    #endregion

    #region AddOrders

    [Fact]
    public void AddOrders_WithValidQuantity_ShouldIncreaseTotal()
    {
        // Arrange
        var batch = Batch.Create(5);

        // Act
        batch.AddOrders(3);

        // Assert
        batch.TotalOrders.Should().Be(8);
    }

    [Fact]
    public void AddOrders_WithZeroOrNegative_ShouldThrowDomainException()
    {
        // Arrange
        var batch = Batch.Create();

        // Act
        Action act = () => batch.AddOrders(0);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(BatchErrors.CannotAddNonPositiveQuantityOfOrders);
    }

    [Fact]
    public void AddOrders_WhenBatchIsClosed_ShouldThrowDomainException()
    {
        // Arrange
        var batch = Batch.Create();
        batch.Close();

        // Act
        Action act = () => batch.AddOrders(5);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(BatchErrors.CannotAddOrdersToClosedBatch);
    }

    #endregion

    #region Close

    [Fact]
    public void Close_WhenOpen_ShouldCloseAndRaiseEvent()
    {
        // Arrange
        var batch = Batch.Create();

        // Act
        batch.Close();

        // Assert
        batch.Status.Should().Be(BatchStatusEnum.Closed);
        batch.ClosedAt.Should().NotBeNull();
        batch.DomainEvents.Should().ContainSingle(e => e is BatchClosed);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldThrowDomainException()
    {
        // Arrange
        var batch = Batch.Create();
        batch.Close();

        // Act
        Action act = () => batch.Close();

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(BatchErrors.CannotCloseAlreadyClosedBatch);
    }

    #endregion
}