using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Customers.GetCustomerById;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Customers;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Customers;

public class GetCustomerByIdHandlerTest : IDisposable {
	private readonly PersistenceDbContext _dbContext;
	private readonly GetCustomerByIdHandler _handler;

	public GetCustomerByIdHandlerTest() {
		DbContextOptions<PersistenceDbContext> options = new DbContextOptionsBuilder<PersistenceDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		_dbContext = new PersistenceDbContext(options);
		_handler = new GetCustomerByIdHandler(_dbContext);
	}

	public void Dispose() {
		_dbContext.Dispose();
	}

	private static CustomerPersistenceModel CreateCustomerPersistenceModel(
		Guid? id = null,
		string fullName = "John Doe",
		string phoneNumber = "1234567890"
	) {
		return new CustomerPersistenceModel {
			Id = id ?? Guid.NewGuid(),
			FullName = fullName,
			PhoneNumber = phoneNumber,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};
	}

	[Fact]
	public async Task Handle_WithExistingCustomerId_ShouldReturnCustomer() {
		// Arrange
		CustomerPersistenceModel newCustomer = CreateCustomerPersistenceModel(
			fullName: "Alice Smith",
			phoneNumber: "5551234567"
		);

		await _dbContext.Customers.AddAsync(newCustomer);
		await _dbContext.SaveChangesAsync();

		var query = new GetCustomerByIdQuery(newCustomer.Id);

		// Act
		Result<CustomerDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(newCustomer.Id);
		result.Value.FullName.Should().Be("Alice Smith");
		result.Value.PhoneNumber.Should().Be("5551234567");
	}

	[Fact]
	public async Task Handle_WithNonExistingCustomerId_ShouldReturnNotFoundError() {
		// Arrange
		var nonExistingId = Guid.NewGuid();
		var query = new GetCustomerByIdQuery(nonExistingId);

		// Act
		Result<CustomerDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Customer", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithNonExistingCustomerIdAndOtherCustomersInDb_ShouldReturnNotFoundError() {
		// Arrange
		CustomerPersistenceModel customer1 = CreateCustomerPersistenceModel();
		CustomerPersistenceModel customer2 = CreateCustomerPersistenceModel();

		await _dbContext.Customers.AddRangeAsync(customer1, customer2);
		await _dbContext.SaveChangesAsync();

		var nonExistingId = Guid.NewGuid();
		var query = new GetCustomerByIdQuery(nonExistingId);

		// Act
		Result<CustomerDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Customer", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithMultipleCustomers_ShouldReturnCorrectCustomer() {
		// Arrange
		CustomerPersistenceModel customer1 = CreateCustomerPersistenceModel(
			fullName: "Alice Smith",
			phoneNumber: "1111111111"
		);
		CustomerPersistenceModel customer2 = CreateCustomerPersistenceModel(
			fullName: "Bob Johnson",
			phoneNumber: "2222222222"
		);
		CustomerPersistenceModel customer3 = CreateCustomerPersistenceModel(
			fullName: "Charlie Brown",
			phoneNumber: "3333333333"
		);

		await _dbContext.Customers.AddRangeAsync(customer1, customer2, customer3);
		await _dbContext.SaveChangesAsync();

		var query = new GetCustomerByIdQuery(customer2.Id);

		// Act
		Result<CustomerDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(customer2.Id);
		result.Value.FullName.Should().Be("Bob Johnson");
		result.Value.PhoneNumber.Should().Be("2222222222");
	}
}
