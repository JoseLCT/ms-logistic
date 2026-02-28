using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Customers.GetAllCustomers;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Customers;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Customers;

public class GetAllCustomersHandlerTest : IDisposable {
    private readonly PersistenceDbContext _dbContext;
    private readonly GetAllCustomersHandler _handler;

    public GetAllCustomersHandlerTest() {
        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
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
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSingleCustomer_ShouldReturnListWithOneCustomer() {
        // Arrange
        var customer = CreateCustomerPersistenceModel();

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllCustomersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(customer.Id);
        result.Value[0].FullName.Should().Be(customer.FullName);
    }

    [Fact]
    public async Task Handle_WithMultipleCustomers_ShouldReturnAllCustomers() {
        // Arrange
        var customer1 = CreateCustomerPersistenceModel();
        var customer2 = CreateCustomerPersistenceModel();

        await _dbContext.Customers.AddRangeAsync(customer1, customer2);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllCustomersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
