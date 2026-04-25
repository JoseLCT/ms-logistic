using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Drivers.GetDriverById;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Drivers.Enums;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Drivers;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Drivers;

public class GetDriverByIdHandlerTest : IDisposable {
	private readonly PersistenceDbContext _dbContext;
	private readonly GetDriverByIdHandler _handler;

	public GetDriverByIdHandlerTest() {
		DbContextOptions<PersistenceDbContext> options = new DbContextOptionsBuilder<PersistenceDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		_dbContext = new PersistenceDbContext(options);
		_handler = new GetDriverByIdHandler(_dbContext);
	}

	public void Dispose() {
		_dbContext.Dispose();
	}

	private static DriverPersistenceModel CreateDriverPersistenceModel(
		Guid? id = null,
		string fullName = "Jane Smith",
		bool isActive = true,
		DriverStatusEnum status = DriverStatusEnum.Available
	) {
		return new DriverPersistenceModel {
			Id = id ?? Guid.NewGuid(),
			FullName = fullName,
			IsActive = isActive,
			Status = status,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};
	}

	[Fact]
	public async Task Handle_WithExistingDriverId_ShouldReturnDriverWithAllFieldsMapped() {
		// Arrange
		DriverPersistenceModel newDriver = CreateDriverPersistenceModel(
			fullName: "Alice Smith",
			isActive: true,
			status: DriverStatusEnum.Available
		);

		await _dbContext.Drivers.AddAsync(newDriver);
		await _dbContext.SaveChangesAsync();

		var query = new GetDriverByIdQuery(newDriver.Id);

		// Act
		Result<DriverDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(newDriver.Id);
		result.Value.FullName.Should().Be("Alice Smith");
		result.Value.IsActive.Should().BeTrue();
		result.Value.Status.Should().Be(DriverStatusEnum.Available);
	}

	[Fact]
	public async Task Handle_WithInactiveDriver_ShouldMapIsActiveAndStatusCorrectly() {
		// Arrange
		DriverPersistenceModel newDriver = CreateDriverPersistenceModel(
			fullName: "Bob Johnson",
			isActive: false,
			status: DriverStatusEnum.Unavailable
		);

		await _dbContext.Drivers.AddAsync(newDriver);
		await _dbContext.SaveChangesAsync();

		var query = new GetDriverByIdQuery(newDriver.Id);

		// Act
		Result<DriverDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(newDriver.Id);
		result.Value.FullName.Should().Be("Bob Johnson");
		result.Value.IsActive.Should().BeFalse();
		result.Value.Status.Should().Be(DriverStatusEnum.Unavailable);
	}

	[Fact]
	public async Task Handle_WithNonExistingDriverId_ShouldReturnNotFoundError() {
		// Arrange
		var nonExistingId = Guid.NewGuid();
		var query = new GetDriverByIdQuery(nonExistingId);

		// Act
		Result<DriverDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Driver", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithNonExistingIdAndOtherDriversInDb_ShouldReturnNotFoundError() {
		// Arrange
		DriverPersistenceModel driver1 = CreateDriverPersistenceModel();
		DriverPersistenceModel driver2 = CreateDriverPersistenceModel();

		await _dbContext.Drivers.AddRangeAsync(driver1, driver2);
		await _dbContext.SaveChangesAsync();

		var nonExistingId = Guid.NewGuid();
		var query = new GetDriverByIdQuery(nonExistingId);

		// Act
		Result<DriverDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Driver", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithMultipleDrivers_ShouldReturnCorrectDriver() {
		// Arrange
		DriverPersistenceModel driver1 = CreateDriverPersistenceModel(
			fullName: "Alice Smith",
			isActive: true,
			status: DriverStatusEnum.Available
		);
		DriverPersistenceModel driver2 = CreateDriverPersistenceModel(
			fullName: "Bob Johnson",
			isActive: false,
			status: DriverStatusEnum.Unavailable
		);
		DriverPersistenceModel driver3 = CreateDriverPersistenceModel(
			fullName: "Charlie Brown",
			isActive: true,
			status: DriverStatusEnum.Available
		);

		await _dbContext.Drivers.AddRangeAsync(driver1, driver2, driver3);
		await _dbContext.SaveChangesAsync();

		var query = new GetDriverByIdQuery(driver2.Id);

		// Act
		Result<DriverDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(driver2.Id);
		result.Value.FullName.Should().Be("Bob Johnson");
		result.Value.IsActive.Should().BeFalse();
		result.Value.Status.Should().Be(DriverStatusEnum.Unavailable);
	}
}
