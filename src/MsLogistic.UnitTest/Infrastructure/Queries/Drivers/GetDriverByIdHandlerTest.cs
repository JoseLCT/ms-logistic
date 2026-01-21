using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Drivers.GetDriverById;
using MsLogistic.Domain.Drivers.Enums;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Drivers;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Drivers;

public class GetDriverByIdHandlerTest : IDisposable
{
    private readonly PersistenceDbContext _dbContext;
    private readonly GetDriverByIdHandler _handler;

    public GetDriverByIdHandlerTest()
    {
        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PersistenceDbContext(options);
        _handler = new GetDriverByIdHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private static DriverPersistenceModel CreateDriverPersistenceModel(
        Guid? id = null,
        string fullName = "Jane Smith",
        bool isActive = true,
        DriverStatusEnum status = DriverStatusEnum.Available
    )
    {
        return new DriverPersistenceModel
        {
            Id = id ?? Guid.NewGuid(),
            FullName = fullName,
            IsActive = isActive,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    [Fact]
    public async Task Handle_WithExistingDriverId_ShouldReturnDriver()
    {
        // Arrange
        var newDriver = CreateDriverPersistenceModel();

        await _dbContext.Drivers.AddAsync(newDriver);
        await _dbContext.SaveChangesAsync();

        var query = new GetDriverByIdQuery(newDriver.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(newDriver.Id);
    }

    [Fact]
    public async Task Handle_WithNonExistingDriverId_ShouldReturnNotFoundError()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var query = new GetDriverByIdQuery(nonExistingId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommonErrors.NotFoundById("Driver", nonExistingId));
    }

    [Fact]
    public async Task Handle_WithMultipleDrivers_ShouldReturnCorrectDriver()
    {
        // Arrange
        var driver1 = CreateDriverPersistenceModel();
        var driver2 = CreateDriverPersistenceModel();

        await _dbContext.Drivers.AddRangeAsync(driver1, driver2);
        await _dbContext.SaveChangesAsync();

        var query = new GetDriverByIdQuery(driver2.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(driver2.Id);
    }
}