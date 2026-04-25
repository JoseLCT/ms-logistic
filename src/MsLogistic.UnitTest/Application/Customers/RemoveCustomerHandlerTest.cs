using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Customers.RemoveCustomer;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Application.Customers;

public class RemoveCustomerHandlerTest {
	private readonly Mock<ICustomerRepository> _customerRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly RemoveCustomerHandler _handler;

	public RemoveCustomerHandlerTest() {
		_customerRepositoryMock = new Mock<ICustomerRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var logger = new Mock<ILogger<RemoveCustomerHandler>>();
		_handler = new RemoveCustomerHandler(
			_customerRepositoryMock.Object,
			_unitOfWorkMock.Object,
			logger.Object
		);
	}

	[Fact]
	public async Task Handle_WhenCustomerExists_ShouldRemoveAndCommit() {
		// Arrange
		var customer = Customer.Create("Juan Perez", PhoneNumberValue.Create("+59112345678"));
		var command = new RemoveCustomerCommand(customer.Id);

		_customerRepositoryMock
			.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(customer);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		_customerRepositoryMock.Verify(r => r.Remove(customer), Times.Once);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenCustomerDoesNotExist_ShouldReturnFailure() {
		// Arrange
		var customerId = Guid.NewGuid();
		var command = new RemoveCustomerCommand(customerId);

		_customerRepositoryMock
			.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Customer?)null);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Customer", customerId));

		_customerRepositoryMock.Verify(r => r.Remove(It.IsAny<Customer>()), Times.Never);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}
