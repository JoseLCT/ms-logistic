using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Customers.GetAllCustomers;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Customers;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Customers;

public class GetAllCustomersHandlerTest : IDisposable {
	private readonly PersistenceDbContext _dbContext;
	private readonly GetAllCustomersHandler _handler;

	public GetAllCustomersHandlerTest() {
		DbContextOptions<PersistenceDbContext> options = new DbContextOptionsBuilder<PersistenceDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		_dbContext = new PersistenceDbContext(options);
		_handler = new GetAllCustomersHandler(_dbContext);
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
	public async Task Handle_WithNoCustomers_ShouldReturnEmptyList() {
		// Arrange
		var query = new GetAllCustomersQuery();

		// Act
		Result<IReadOnlyList<CustomerSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithSingleCustomer_ShouldReturnListWithOneCustomer() {
		// Arrange
		CustomerPersistenceModel customer = CreateCustomerPersistenceModel(
			fullName: "Alice Smith"
		);

		await _dbContext.Customers.AddAsync(customer);
		await _dbContext.SaveChangesAsync();

		var query = new GetAllCustomersQuery();

		// Act
		Result<IReadOnlyList<CustomerSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().HaveCount(1);
		result.Value[0].Id.Should().Be(customer.Id);
		result.Value[0].FullName.Should().Be("Alice Smith");
	}

	[Fact]
	public async Task Handle_WithMultipleCustomers_ShouldReturnAllCustomers() {
		// Arrange
		CustomerPersistenceModel customer1 = CreateCustomerPersistenceModel(
			fullName: "Alice Smith"
		);
		CustomerPersistenceModel customer2 = CreateCustomerPersistenceModel(
			fullName: "Bob Johnson"
		);
		CustomerPersistenceModel customer3 = CreateCustomerPersistenceModel(
			fullName: "Charlie Brown"
		);

		await _dbContext.Customers.AddRangeAsync(customer1, customer2, customer3);
		await _dbContext.SaveChangesAsync();

		var query = new GetAllCustomersQuery();

		// Act
		Result<IReadOnlyList<CustomerSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(3);
		result.Value.Should().BeEquivalentTo([
			new CustomerSummaryDto(customer1.Id, "Alice Smith"),
			new CustomerSummaryDto(customer2.Id, "Bob Johnson"),
			new CustomerSummaryDto(customer3.Id, "Charlie Brown")
		]);
	}
}
