using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.DeliveryZones.GetDeliveryZoneById;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.DeliveryZones;
using NetTopologySuite.Geometries;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.DeliveryZones;

public class GetDeliveryZoneByIdHandlerTest : IDisposable {
	private readonly PersistenceDbContext _dbContext;
	private readonly GetDeliveryZoneByIdHandler _handler;

	public GetDeliveryZoneByIdHandlerTest() {
		DbContextOptions<PersistenceDbContext> options = new DbContextOptionsBuilder<PersistenceDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		_dbContext = new PersistenceDbContext(options);
		_handler = new GetDeliveryZoneByIdHandler(_dbContext);
	}

	public void Dispose() {
		_dbContext.Dispose();
	}

	private static DeliveryZonePersistenceModel CreateDeliveryZonePersistenceModel(
		Guid? id = null,
		Guid? driverId = null,
		string code = "ABC-123",
		string name = "Zone A",
		Polygon? boundaries = null
	) {
		return new DeliveryZonePersistenceModel {
			Id = id ?? Guid.NewGuid(),
			DriverId = driverId,
			Code = code,
			Name = name,
			Boundaries = boundaries ?? new Polygon(new LinearRing(new[] {
				new Coordinate(0, 0),
				new Coordinate(0, 1),
				new Coordinate(1, 1),
				new Coordinate(1, 0),
				new Coordinate(0, 0)
			})),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};
	}

	[Fact]
	public async Task Handle_WithExistingDeliveryZoneId_ShouldReturnDeliveryZoneWithAllFieldsMapped() {
		// Arrange
		var driverId = Guid.NewGuid();
		DeliveryZonePersistenceModel newDeliveryZone = CreateDeliveryZonePersistenceModel(
			driverId: driverId,
			code: "NORTH-001",
			name: "Northern Zone"
		);

		await _dbContext.DeliveryZones.AddAsync(newDeliveryZone);
		await _dbContext.SaveChangesAsync();

		var query = new GetDeliveryZoneByIdQuery(newDeliveryZone.Id);

		// Act
		Result<DeliveryZoneDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(newDeliveryZone.Id);
		result.Value.DriverId.Should().Be(driverId);
		result.Value.Code.Should().Be("NORTH-001");
		result.Value.Name.Should().Be("Northern Zone");
		result.Value.Boundaries.Should().HaveCount(5);
	}

	[Fact]
	public async Task Handle_WithExistingDeliveryZoneWithoutDriver_ShouldReturnDeliveryZoneWithNullDriverId() {
		// Arrange
		DeliveryZonePersistenceModel newDeliveryZone = CreateDeliveryZonePersistenceModel(
			driverId: null
		);

		await _dbContext.DeliveryZones.AddAsync(newDeliveryZone);
		await _dbContext.SaveChangesAsync();

		var query = new GetDeliveryZoneByIdQuery(newDeliveryZone.Id);

		// Act
		Result<DeliveryZoneDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.DriverId.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithExistingDeliveryZone_ShouldMapCoordinatesSwappingXAndY() {
		// Arrange
		var polygon = new Polygon(new LinearRing([
			new Coordinate(-68.15, -16.50),
			new Coordinate(-68.10, -16.50),
			new Coordinate(-68.10, -16.45),
			new Coordinate(-68.15, -16.45),
			new Coordinate(-68.15, -16.50)
		]));

		DeliveryZonePersistenceModel newDeliveryZone = CreateDeliveryZonePersistenceModel(
			boundaries: polygon
		);

		await _dbContext.DeliveryZones.AddAsync(newDeliveryZone);
		await _dbContext.SaveChangesAsync();

		var query = new GetDeliveryZoneByIdQuery(newDeliveryZone.Id);

		// Act
		Result<DeliveryZoneDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Boundaries.Should().HaveCount(5);

		result.Value.Boundaries[0].Latitude.Should().Be(-16.50);
		result.Value.Boundaries[0].Longitude.Should().Be(-68.15);

		result.Value.Boundaries[2].Latitude.Should().Be(-16.45);
		result.Value.Boundaries[2].Longitude.Should().Be(-68.10);
	}

	[Fact]
	public async Task Handle_WithNonExistingDeliveryZoneId_ShouldReturnNotFoundError() {
		// Arrange
		var nonExistingId = Guid.NewGuid();
		var query = new GetDeliveryZoneByIdQuery(nonExistingId);

		// Act
		Result<DeliveryZoneDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("DeliveryZone", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithNonExistingIdAndOtherZonesInDb_ShouldReturnNotFoundError() {
		// Arrange
		DeliveryZonePersistenceModel zone1 = CreateDeliveryZonePersistenceModel();
		DeliveryZonePersistenceModel zone2 = CreateDeliveryZonePersistenceModel();

		await _dbContext.DeliveryZones.AddRangeAsync(zone1, zone2);
		await _dbContext.SaveChangesAsync();

		var nonExistingId = Guid.NewGuid();
		var query = new GetDeliveryZoneByIdQuery(nonExistingId);

		// Act
		Result<DeliveryZoneDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("DeliveryZone", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithMultipleDeliveryZones_ShouldReturnCorrectDeliveryZone() {
		// Arrange
		DeliveryZonePersistenceModel zone1 = CreateDeliveryZonePersistenceModel(
			code: "NORTH-001",
			name: "Northern Zone"
		);
		DeliveryZonePersistenceModel zone2 = CreateDeliveryZonePersistenceModel(
			code: "SOUTH-002",
			name: "Southern Zone"
		);
		DeliveryZonePersistenceModel zone3 = CreateDeliveryZonePersistenceModel(
			code: "EAST-003",
			name: "Eastern Zone"
		);

		await _dbContext.DeliveryZones.AddRangeAsync(zone1, zone2, zone3);
		await _dbContext.SaveChangesAsync();

		var query = new GetDeliveryZoneByIdQuery(zone2.Id);

		// Act
		Result<DeliveryZoneDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(zone2.Id);
		result.Value.Code.Should().Be("SOUTH-002");
		result.Value.Name.Should().Be("Southern Zone");
	}
}
