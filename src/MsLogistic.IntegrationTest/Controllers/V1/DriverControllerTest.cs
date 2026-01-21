using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MsLogistic.Application.Drivers.GetAllDrivers;
using MsLogistic.Application.Drivers.GetDriverById;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.IntegrationTest.Fixtures;
using MsLogistic.WebApi.Contracts.V1.Drivers;
using Xunit;

namespace MsLogistic.IntegrationTest.Controllers.V1;

public class DriverControllerTest : IClassFixture<WebApplicationFactoryFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactoryFixture _factory;
    private readonly List<Guid> _createdDriverIds = [];

    public DriverControllerTest(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (!_createdDriverIds.Any())
        {
            return;
        }

        using var scope = _factory.Services.CreateScope();
        var persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

        var driversToDelete = await persistenceDb.Drivers
            .Where(d => _createdDriverIds.Contains(d.Id))
            .ToListAsync();

        if (driversToDelete.Any())
        {
            persistenceDb.Drivers.RemoveRange(driversToDelete);
            await persistenceDb.SaveChangesAsync();
        }
    }

    #region Create Driver Tests

    [Fact]
    public async Task CreateDriver_WithValidData_ShouldCreateDriverSuccessfully()
    {
        // Arrange
        var contract = new CreateDriverContract
        {
            FullName = "Juan Pérez García"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/drivers", contract);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var driverId = await response.Content.ReadFromJsonAsync<Guid>();
        driverId.Should().NotBeEmpty();
        _createdDriverIds.Add(driverId);

        await VerifyDriverInPersistenceDb(driverId, contract.FullName);
    }

    [Fact]
    public async Task CreateDriver_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var contract = new CreateDriverContract
        {
            FullName = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/drivers", contract);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateDriver_WithNullName_ShouldReturnBadRequest()
    {
        // Arrange
        var contract = new CreateDriverContract
        {
            FullName = null!
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/drivers", contract);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Update Driver Tests

    [Fact]
    public async Task UpdateDriver_WithValidData_ShouldUpdateDriverSuccessfully()
    {
        // Arrange
        var createContract = new CreateDriverContract { FullName = "María López" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/drivers", createContract);
        var driverId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        _createdDriverIds.Add(driverId);

        var updateContract = new UpdateDriverContract
        {
            FullName = "María López Rodríguez",
            IsActive = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/drivers/{driverId}", updateContract);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await VerifyDriverInPersistenceDb(driverId, "María López Rodríguez", isActive: false);
    }

    [Fact]
    public async Task UpdateDriver_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var updateContract = new UpdateDriverContract
        {
            FullName = "Conductor Inexistente",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/drivers/{nonExistingId}", updateContract);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Delete Driver Tests

    [Fact]
    public async Task RemoveDriver_WithExistingId_ShouldDeleteDriverSuccessfully()
    {
        // Arrange
        var createContract = new CreateDriverContract { FullName = "Pedro Sánchez" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/drivers", createContract);
        var driverId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        _createdDriverIds.Add(driverId);

        // Act
        var response = await _client.DeleteAsync($"/api/v1/drivers/{driverId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await VerifyDriverDoesNotExist(driverId);
    }

    [Fact]
    public async Task RemoveDriver_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/drivers/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Get Driver Tests

    [Fact]
    public async Task GetDriverById_WithExistingId_ShouldReturnDriver()
    {
        // Arrange
        var createContract = new CreateDriverContract { FullName = "Ana Martínez" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/drivers", createContract);
        var driverId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        _createdDriverIds.Add(driverId);

        // Act
        var response = await _client.GetAsync($"/api/v1/drivers/{driverId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var driver = await response.Content.ReadFromJsonAsync<DriverDetailDto>();
        driver.Should().NotBeNull();
        driver.Id.Should().Be(driverId);
        driver.FullName.Should().Be("Ana Martínez");
    }

    [Fact]
    public async Task GetDriverById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/drivers/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllDrivers_ShouldReturnListOfDrivers()
    {
        // Arrange
        var driver1 = new CreateDriverContract { FullName = "Conductor 1" };
        var driver2 = new CreateDriverContract { FullName = "Conductor 2" };
        var driver3 = new CreateDriverContract { FullName = "Conductor 3" };

        var response1 = await _client.PostAsJsonAsync("/api/v1/drivers", driver1);
        var response2 = await _client.PostAsJsonAsync("/api/v1/drivers", driver2);
        var response3 = await _client.PostAsJsonAsync("/api/v1/drivers", driver3);

        _createdDriverIds.Add(await response1.Content.ReadFromJsonAsync<Guid>());
        _createdDriverIds.Add(await response2.Content.ReadFromJsonAsync<Guid>());
        _createdDriverIds.Add(await response3.Content.ReadFromJsonAsync<Guid>());

        // Act
        var response = await _client.GetAsync("/api/v1/drivers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var drivers = await response.Content.ReadFromJsonAsync<List<DriverSummaryDto>>();
        drivers.Should().NotBeNull();
        drivers.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    #endregion

    #region Complete CRUD Flow

    [Fact]
    public async Task CompleteDriverCRUDFlow_ShouldWorkEndToEnd()
    {
        // CREATE
        var createContract = new CreateDriverContract { FullName = "Test Driver CRUD" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/drivers", createContract);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var driverId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        _createdDriverIds.Add(driverId);

        // READ
        var getResponse = await _client.GetAsync($"/api/v1/drivers/{driverId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var driver = await getResponse.Content.ReadFromJsonAsync<DriverDetailDto>();
        driver.Should().NotBeNull();
        driver!.FullName.Should().Be("Test Driver CRUD");

        // UPDATE
        var updateContract = new UpdateDriverContract
        {
            FullName = "Test Driver Updated",
            IsActive = false
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/drivers/{driverId}", updateContract);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // VERIFY UPDATE
        var getUpdatedResponse = await _client.GetAsync($"/api/v1/drivers/{driverId}");
        var updatedDriver = await getUpdatedResponse.Content.ReadFromJsonAsync<DriverDetailDto>();
        updatedDriver.Should().NotBeNull();
        updatedDriver!.FullName.Should().Be("Test Driver Updated");
        updatedDriver.IsActive.Should().BeFalse();

        // DELETE
        var deleteResponse = await _client.DeleteAsync($"/api/v1/drivers/{driverId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // VERIFY DELETION
        var getDeletedResponse = await _client.GetAsync($"/api/v1/drivers/{driverId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helper Methods

    private async Task VerifyDriverInPersistenceDb(Guid driverId, string expectedFullName, bool? isActive = null)
    {
        using var scope = _factory.Services.CreateScope();
        var persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

        var driver = await persistenceDb.Drivers
            .FirstOrDefaultAsync(d => d.Id == driverId);

        driver.Should().NotBeNull();
        driver!.FullName.Should().Be(expectedFullName);

        if (isActive.HasValue)
        {
            driver.IsActive.Should().Be(isActive.Value);
        }
    }

    private async Task VerifyDriverDoesNotExist(Guid driverId)
    {
        using var scope = _factory.Services.CreateScope();
        var persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

        var driverExists = await persistenceDb.Drivers
            .AnyAsync(d => d.Id == driverId);

        driverExists.Should().BeFalse();
    }

    #endregion
}