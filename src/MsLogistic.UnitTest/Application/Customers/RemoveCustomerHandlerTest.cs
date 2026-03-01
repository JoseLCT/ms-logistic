using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Customers.RemoveCustomer;
using MsLogistic.Core.Interfaces;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Domain.Shared.Errors;
using Xunit;

namespace MsLogistic.UnitTest.Application.Customers;

public class RemoveCustomerHandlerTest {
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<RemoveCustomerHandler>> _logger;
    private readonly RemoveCustomerHandler _handler;

    public RemoveCustomerHandlerTest() {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<RemoveCustomerHandler>>();
        _handler = new RemoveCustomerHandler(
            _customerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _logger.Object
        );
    }

    private static Customer CreateValidCustomer(string fullName = "Juan Perez") {
        return Customer.Create(fullName, null);
    }

    [Fact]
    public async Task Handle_WhenCustomerExists_ShouldRemoveCustomerAndReturnSuccessResult() {
        // Arrange
        var customer = CreateValidCustomer();
        var command = new RemoveCustomerCommand(customer.Id);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(customer.Id);

        _customerRepositoryMock.Verify(
            x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _customerRepositoryMock.Verify(
            x => x.Remove(customer),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenCustomerDoesNotExist_ShouldReturnFailureResult() {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new RemoveCustomerCommand(customerId);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(CommonErrors.NotFoundById("Customer", customerId));

        _customerRepositoryMock.Verify(
            x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _customerRepositoryMock.Verify(
            x => x.Remove(It.IsAny<Customer>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
