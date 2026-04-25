using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MsLogistic.Application.Drivers.GetAllDrivers;
using MsLogistic.Application.Drivers.GetDriverById;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.IntegrationTest.Fixtures;
using MsLogistic.IntegrationTest.Helpers;
using MsLogistic.WebApi.Contracts.V1.Drivers;
using Xunit;

namespace MsLogistic.IntegrationTest.Controllers.V1;

public class DriverControllerTest : IClassFixture<WebApplicationFactoryFixture>, IAsyncLifetime {
	private const string BaseUrl = "/api/logistic/v1/drivers";
	private readonly HttpClient _client;
	private readonly WebApplicationFactoryFixture _factory;
	private readonly List<Guid> _createdDriverIds = [];

	public DriverControllerTest(WebApplicationFactoryFixture factory) {
		_factory = factory;
		_client = factory.CreateClient();
	}

	public Task InitializeAsync() => Task.CompletedTask;

	public async Task DisposeAsync() {
		if (_createdDriverIds.Count == 0) {
			return;
		}

		using IServiceScope scope = _factory.Services.CreateScope();
		PersistenceDbContext persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

		List<DriverPersistenceModel> driversToDelete = await persistenceDb.Drivers
			.Where(d => _createdDriverIds.Contains(d.Id))
			.ToListAsync();

		if (driversToDelete.Count != 0) {
			persistenceDb.Drivers.RemoveRange(driversToDelete);
			await persistenceDb.SaveChangesAsync();
		}
	}

	#region Create Driver Tests

	[Fact]
	public async Task CreateDriver_WithValidData_ShouldCreateDriverSuccessfully() {
		// Arrange
		var contract = new CreateDriverContract {
			FullName = "Juan Perez"
		};

		// Act
		HttpResponseMessage response = await _client.PostAsJsonAsync(BaseUrl, contract);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Created);

		ApiResponse<Guid>? result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
		result.Should().NotBeNull();
		result!.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();
		_createdDriverIds.Add(result.Value);

		await VerifyDriverInPersistenceDb(result.Value, contract.FullName);
	}

	[Fact]
	public async Task CreateDriver_WithEmptyName_ShouldReturnBadRequest() {
		// Arrange
		var contract = new CreateDriverContract {
			FullName = ""
		};

		// Act
		HttpResponseMessage response = await _client.PostAsJsonAsync(BaseUrl, contract);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task CreateDriver_WithNullName_ShouldReturnBadRequest() {
		// Arrange
		var contract = new CreateDriverContract {
			FullName = null!
		};

		// Act
		HttpResponseMessage response = await _client.PostAsJsonAsync(BaseUrl, contract);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	#endregion

	#region Update Driver Tests

	[Fact]
	public async Task UpdateDriver_WithValidData_ShouldUpdateDriverSuccessfully() {
		// Arrange
		Guid driverId = await CreateDriverAndTrack("Maria Lopez");

		var updateContract = new UpdateDriverContract {
			FullName = "Maria Lopez Rodriguez",
			IsActive = false
		};

		// Act
		HttpResponseMessage response = await _client.PutAsJsonAsync($"{BaseUrl}/{driverId}", updateContract);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		await VerifyDriverInPersistenceDb(driverId, "Maria Lopez Rodriguez", isActive: false);
	}

	[Fact]
	public async Task UpdateDriver_WithNonExistingId_ShouldReturnNotFound() {
		// Arrange
		var nonExistingId = Guid.NewGuid();
		var updateContract = new UpdateDriverContract {
			FullName = "Conductor Inexistente",
			IsActive = true
		};

		// Act
		HttpResponseMessage response = await _client.PutAsJsonAsync($"{BaseUrl}/{nonExistingId}", updateContract);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	#endregion

	#region Delete Driver Tests

	[Fact]
	public async Task RemoveDriver_WithExistingId_ShouldDeleteDriverSuccessfully() {
		// Arrange
		Guid driverId = await CreateDriverAndTrack("Pedro Sanchez");

		// Act
		HttpResponseMessage response = await _client.DeleteAsync($"{BaseUrl}/{driverId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		await VerifyDriverDoesNotExist(driverId);
	}

	[Fact]
	public async Task RemoveDriver_WithNonExistingId_ShouldReturnNotFound() {
		// Arrange
		var nonExistingId = Guid.NewGuid();

		// Act
		HttpResponseMessage response = await _client.DeleteAsync($"{BaseUrl}/{nonExistingId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	#endregion

	#region Get Driver Tests

	[Fact]
	public async Task GetDriverById_WithExistingId_ShouldReturnDriver() {
		// Arrange
		Guid driverId = await CreateDriverAndTrack("Ana Martinez");

		// Act
		HttpResponseMessage response = await _client.GetAsync($"{BaseUrl}/{driverId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		ApiResponse<DriverDetailDto>? result = await response.Content.ReadFromJsonAsync<ApiResponse<DriverDetailDto>>();
		result.Should().NotBeNull();
		result!.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Id.Should().Be(driverId);
		result.Value.FullName.Should().Be("Ana Martinez");
	}

	[Fact]
	public async Task GetDriverById_WithNonExistingId_ShouldReturnNotFound() {
		// Arrange
		var nonExistingId = Guid.NewGuid();

		// Act
		HttpResponseMessage response = await _client.GetAsync($"{BaseUrl}/{nonExistingId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetAllDrivers_ShouldReturnListOfDrivers() {
		// Arrange
		await CreateDriverAndTrack("Conductor 1");
		await CreateDriverAndTrack("Conductor 2");
		await CreateDriverAndTrack("Conductor 3");

		// Act
		HttpResponseMessage response = await _client.GetAsync(BaseUrl);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		ApiResponse<List<DriverSummaryDto>>? result =
			await response.Content.ReadFromJsonAsync<ApiResponse<List<DriverSummaryDto>>>();
		result.Should().NotBeNull();
		result!.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().HaveCountGreaterThanOrEqualTo(3);
	}

	#endregion

	#region Complete CRUD Flow

	[Fact]
	public async Task CompleteDriverCRUDFlow_ShouldWorkEndToEnd() {
		// CREATE
		Guid driverId = await CreateDriverAndTrack("Test Driver CRUD");

		// READ
		HttpResponseMessage getResponse = await _client.GetAsync($"{BaseUrl}/{driverId}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		ApiResponse<DriverDetailDto>? driver = await getResponse.Content.ReadFromJsonAsync<ApiResponse<DriverDetailDto>>();
		driver.Should().NotBeNull();
		driver!.Value!.FullName.Should().Be("Test Driver CRUD");

		// UPDATE
		var updateContract = new UpdateDriverContract {
			FullName = "Test Driver Updated",
			IsActive = false
		};
		HttpResponseMessage updateResponse = await _client.PutAsJsonAsync($"{BaseUrl}/{driverId}", updateContract);
		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		// VERIFY UPDATE
		HttpResponseMessage getUpdatedResponse = await _client.GetAsync($"{BaseUrl}/{driverId}");
		ApiResponse<DriverDetailDto>? updatedDriver =
			await getUpdatedResponse.Content.ReadFromJsonAsync<ApiResponse<DriverDetailDto>>();
		updatedDriver.Should().NotBeNull();
		updatedDriver!.Value!.FullName.Should().Be("Test Driver Updated");
		updatedDriver.Value.IsActive.Should().BeFalse();

		// DELETE
		HttpResponseMessage deleteResponse = await _client.DeleteAsync($"{BaseUrl}/{driverId}");
		deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		// VERIFY DELETION
		HttpResponseMessage getDeletedResponse = await _client.GetAsync($"{BaseUrl}/{driverId}");
		getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	#endregion

	#region Helper Methods

	private async Task<Guid> CreateDriverAndTrack(string fullName) {
		var contract = new CreateDriverContract { FullName = fullName };
		HttpResponseMessage response = await _client.PostAsJsonAsync(BaseUrl, contract);
		response.StatusCode.Should().Be(HttpStatusCode.Created);

		ApiResponse<Guid>? result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
		result.Should().NotBeNull();
		result!.Value.Should().NotBeEmpty();
		_createdDriverIds.Add(result.Value);
		return result.Value;
	}

	private async Task VerifyDriverInPersistenceDb(Guid driverId, string expectedFullName, bool? isActive = null) {
		using IServiceScope scope = _factory.Services.CreateScope();
		PersistenceDbContext persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

		DriverPersistenceModel? driver = await persistenceDb.Drivers
			.FirstOrDefaultAsync(d => d.Id == driverId);

		driver.Should().NotBeNull();
		driver!.FullName.Should().Be(expectedFullName);

		if (isActive.HasValue) {
			driver.IsActive.Should().Be(isActive.Value);
		}
	}

	private async Task VerifyDriverDoesNotExist(Guid driverId) {
		using IServiceScope scope = _factory.Services.CreateScope();
		PersistenceDbContext persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

		bool driverExists = await persistenceDb.Drivers
			.AnyAsync(d => d.Id == driverId);

		driverExists.Should().BeFalse();
	}

	#endregion
}
