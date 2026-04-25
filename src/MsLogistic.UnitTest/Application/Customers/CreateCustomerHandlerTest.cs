using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Customers.CreateCustomer;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Customers.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Application.Customers;

public class CreateCustomerHandlerTest {
	private readonly Mock<ICustomerRepository> _customerRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly CreateCustomerHandler _handler;

	public CreateCustomerHandlerTest() {
		_customerRepositoryMock = new Mock<ICustomerRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<CreateCustomerHandler>>();
		_handler = new CreateCustomerHandler(_customerRepositoryMock.Object, _unitOfWorkMock.Object,
			loggerMock.Object);
	}

	[Fact]
	public async Task Handle_WithValidCommandIncludingPhoneNumber_ShouldCreateCustomerAndCommit() {
		// Arrange
		const string fullName = "Juan Perez";
		const string phoneNumber = "+591 12345678";
		var command = new CreateCustomerCommand(fullName, phoneNumber);

		Customer? addedCustomer = null;
		_customerRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
			.Callback<Customer, CancellationToken>((c, _) => addedCustomer = c)
			.Returns(Task.CompletedTask);

		// Act
		Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();

		addedCustomer.Should().NotBeNull();
		addedCustomer.FullName.Should().Be(fullName);
		addedCustomer.PhoneNumber.Should().NotBeNull();
		result.Value.Should().Be(addedCustomer.Id);

		_customerRepositoryMock.Verify(
			x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
			Times.Once
		);
		_unitOfWorkMock.Verify(
			x => x.CommitAsync(It.IsAny<CancellationToken>()),
			Times.Once
		);
	}

	[Fact]
	public async Task Handle_WithNullPhoneNumber_ShouldCreateCustomerWithoutPhone() {
		// Arrange
		const string fullName = "Jane Smith";
		var command = new CreateCustomerCommand(fullName, null);

		Customer? addedCustomer = null;
		_customerRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
			.Callback<Customer, CancellationToken>((c, _) => addedCustomer = c)
			.Returns(Task.CompletedTask);

		// Act
		Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		addedCustomer.Should().NotBeNull();
		addedCustomer.FullName.Should().Be(fullName);
		addedCustomer.PhoneNumber.Should().BeNull();

		_customerRepositoryMock.Verify(
			x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
			Times.Once
		);
		_unitOfWorkMock.Verify(
			x => x.CommitAsync(It.IsAny<CancellationToken>()),
			Times.Once
		);
	}
}
