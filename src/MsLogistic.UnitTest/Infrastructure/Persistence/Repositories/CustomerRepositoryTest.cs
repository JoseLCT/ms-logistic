using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Shared.ValueObjects;
using MsLogistic.Infrastructure.Persistence.DomainModel;
using MsLogistic.Infrastructure.Persistence.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Persistence.Repositories;

public class CustomerRepositoryTest : IDisposable {
    private readonly DomainDbContext _dbContext;
    private readonly CustomerRepository _repository;

    public CustomerRepositoryTest() {
        var options = new DbContextOptionsBuilder<DomainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new DomainDbContext(options);
        _repository = new CustomerRepository(_dbContext);
    }

    public void Dispose() {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private static Customer CreateValidCustomer(
        string fullName = "John Doe",
        string phoneNumber = "+59112345678"
    ) {
        var phone = PhoneNumberValue.Create(phoneNumber);
        return Customer.Create(fullName, phone);
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ShouldReturnCustomer() {
        // Arrange
        var customer = CreateValidCustomer();
        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customer.Id);
        result.FullName.Should().Be(customer.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ShouldReturnNull() {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_WhenCustomersExist_ShouldReturnAllCustomers() {
        // Arrange
        var customer1 = CreateValidCustomer("Mike Brown");
        var customer2 = CreateValidCustomer("Jane Smith");
        var customer3 = CreateValidCustomer("Bob Johnson");

        await _dbContext.Customers.AddRangeAsync(customer1, customer2, customer3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Id == customer1.Id);
        result.Should().Contain(c => c.Id == customer2.Id);
        result.Should().Contain(c => c.Id == customer3.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoCustomers_ShouldReturnEmptyList() {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldAddCustomerToDatabase() {
        // Arrange
        var customer = CreateValidCustomer();

        // Act
        await _repository.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedCustomer = await _dbContext.Customers.FindAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer.Id.Should().Be(customer.Id);
        savedCustomer.FullName.Should().Be(customer.FullName);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ShouldUpdateCustomerInDatabase() {
        // Arrange
        var customer = CreateValidCustomer();
        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(customer).State = EntityState.Detached;

        // Act
        var customerToUpdate = await _dbContext.Customers.FindAsync(customer.Id);
        customerToUpdate!.SetFullName("Juan Perez");
        _repository.Update(customerToUpdate);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedCustomer = await _dbContext.Customers.FindAsync(customer.Id);
        updatedCustomer!.FullName.Should().Be("Juan Perez");
    }

    #endregion

    #region Remove

    [Fact]
    public async Task Remove_ShouldDeleteCustomerFromDatabase() {
        // Arrange
        var customer = CreateValidCustomer();
        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        // Act
        _repository.Remove(customer);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedCustomer = await _dbContext.Customers.FindAsync(customer.Id);
        deletedCustomer.Should().BeNull();
    }

    #endregion
}
