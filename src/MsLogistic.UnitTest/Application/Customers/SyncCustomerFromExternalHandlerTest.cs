using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Customers.SyncCustomerFromExternal;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Application.Customers;

public class SyncCustomerFromExternalHandlerTest {
	private readonly Mock<ICustomerRepository> _customerRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly SyncCustomerFromExternalHandler _handler;

	public SyncCustomerFromExternalHandlerTest() {
		_customerRepositoryMock = new Mock<ICustomerRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<SyncCustomerFromExternalHandler>>();
		_handler = new SyncCustomerFromExternalHandler(
			_customerRepositoryMock.Object,
			_unitOfWorkMock.Object,
			loggerMock.Object
		);
	}

	[Fact]
	public async Task Handle_WhenCustomerDoesNotExist_ShouldCreateNewCustomer() {
		// Arrange
		var externalId = Guid.NewGuid();
		var command = new SyncCustomerFromExternalCommand(
			ExternalId: externalId,
			FullName: "Juan Perez",
			PhoneNumber: "+59112345678"
		);

		_customerRepositoryMock
			.Setup(r => r.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Customer?)null);

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
		addedCustomer.FullName.Should().Be("Juan Perez");
		addedCustomer.ExternalId.Should().Be(externalId);
		addedCustomer.PhoneNumber.Should().NotBeNull();
		result.Value.Should().Be(addedCustomer.Id);

		_customerRepositoryMock.Verify(
			r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
			Times.Once);
		_customerRepositoryMock.Verify(r => r.Update(It.IsAny<Customer>()), Times.Never);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenCustomerDoesNotExistAndNoPhoneNumber_ShouldCreateCustomerWithoutPhone() {
		// Arrange
		var externalId = Guid.NewGuid();
		var command = new SyncCustomerFromExternalCommand(
			ExternalId: externalId,
			FullName: "Jane Smith",
			PhoneNumber: null
		);

		_customerRepositoryMock
			.Setup(r => r.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Customer?)null);

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
		addedCustomer.FullName.Should().Be("Jane Smith");
		addedCustomer.PhoneNumber.Should().BeNull();

		_customerRepositoryMock.Verify(
			r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
			Times.Once);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenCustomerExists_ShouldUpdateCustomer() {
		// Arrange
		var externalId = Guid.NewGuid();
		var existingCustomer = Customer.Create(
			"Old Name",
			PhoneNumberValue.Create("+59100000000"),
			externalId
		);

		var command = new SyncCustomerFromExternalCommand(
			ExternalId: externalId,
			FullName: "Updated Name",
			PhoneNumber: "+59112345678"
		);

		_customerRepositoryMock
			.Setup(r => r.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingCustomer);

		// Act
		Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().Be(existingCustomer.Id);

		existingCustomer.FullName.Should().Be("Updated Name");
		existingCustomer.PhoneNumber.Should().NotBeNull();

		_customerRepositoryMock.Verify(r => r.Update(existingCustomer), Times.Once);
		_customerRepositoryMock.Verify(
			r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenCustomerExistsAndNoPhoneNumber_ShouldUpdateCustomerClearingPhone() {
		// Arrange
		var externalId = Guid.NewGuid();
		var existingCustomer = Customer.Create(
			"Old Name",
			PhoneNumberValue.Create("+59100000000"),
			externalId
		);

		var command = new SyncCustomerFromExternalCommand(
			ExternalId: externalId,
			FullName: "Updated Name",
			PhoneNumber: null
		);

		_customerRepositoryMock
			.Setup(r => r.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingCustomer);

		// Act
		Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		existingCustomer.FullName.Should().Be("Updated Name");
		existingCustomer.PhoneNumber.Should().BeNull();

		_customerRepositoryMock.Verify(r => r.Update(existingCustomer), Times.Once);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}
