using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Customers.UpdateCustomer;
using MsLogistic.Core.Interfaces;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Domain.Shared.Errors;
using Xunit;

namespace MsLogistic.UnitTest.Application.Customers;

public class UpdateCustomerHandlerTest {
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UpdateCustomerHandler>> _logger;
    private readonly UpdateCustomerHandler _handler;

    public UpdateCustomerHandlerTest() {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<UpdateCustomerHandler>>();
        _handler = new UpdateCustomerHandler(
            _customerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _logger.Object
        );
    }

    private static Customer CreateValidCustomer(string fullName = "Juan Perez") {
        return Customer.Create(fullName, null);
    }

    [Fact]
    public async Task Handle_WhenCustomerExists_ShouldUpdateCustomerAndReturnSuccessResult() {
        // Arrange
        var customer = CreateValidCustomer();
        const string fullName = "Maria Lopez";
        const string phoneNumber = "+59177654321";
        var command = new UpdateCustomerCommand(customer.Id, fullName, phoneNumber);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(customer.Id);
        customer.FullName.Should().Be(fullName);
        customer.PhoneNumber.Should().NotBeNull();
        customer.PhoneNumber!.Value.Should().Be(phoneNumber);

        _customerRepositoryMock.Verify(
            x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _customerRepositoryMock.Verify(
            x => x.Update(customer),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenCustomerExistsWithNullPhoneNumber_ShouldUpdateCustomerWithoutPhoneNumber() {
        // Arrange
        var customer = CreateValidCustomer();
        const string fullName = "Carlos Gomez";
        var command = new UpdateCustomerCommand(customer.Id, fullName, null);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(customer.Id);
        customer.FullName.Should().Be(fullName);
        customer.PhoneNumber.Should().BeNull();

        _customerRepositoryMock.Verify(
            x => x.Update(customer),
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
        var command = new UpdateCustomerCommand(customerId, "Ana Martinez", "+59178888888");

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
            x => x.Update(It.IsAny<Customer>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
