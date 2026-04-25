using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.DeliveryZones.GetAllDeliveryZones;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.DeliveryZones;
using NetTopologySuite.Geometries;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.DeliveryZones;

public class GetAllDeliveryZonesHandlerTest : IDisposable {
	private readonly PersistenceDbContext _dbContext;
	private readonly GetAllDeliveryZonesHandler _handler;

	public GetAllDeliveryZonesHandlerTest() {
		DbContextOptions<PersistenceDbContext> options = new DbContextOptionsBuilder<PersistenceDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		_dbContext = new PersistenceDbContext(options);
		_handler = new GetAllDeliveryZonesHandler(_dbContext);
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
	public async Task Handle_WithNoDeliveryZones_ShouldReturnEmptyList() {
		// Arrange
		var query = new GetAllDeliveryZonesQuery();

		// Act
		Result<IReadOnlyList<DeliveryZoneSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithSingleDeliveryZone_ShouldReturnListWithOneDeliveryZone() {
		// Arrange
		DeliveryZonePersistenceModel deliveryZone = CreateDeliveryZonePersistenceModel(
			code: "NORTH-001",
			name: "Northern Zone"
		);

		await _dbContext.DeliveryZones.AddAsync(deliveryZone);
		await _dbContext.SaveChangesAsync();

		var query = new GetAllDeliveryZonesQuery();

		// Act
		Result<IReadOnlyList<DeliveryZoneSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().HaveCount(1);
		result.Value[0].Id.Should().Be(deliveryZone.Id);
		result.Value[0].Code.Should().Be("NORTH-001");
		result.Value[0].Name.Should().Be("Northern Zone");
	}

	[Fact]
	public async Task Handle_WithMultipleDeliveryZones_ShouldReturnAllDeliveryZones() {
		// Arrange
		DeliveryZonePersistenceModel deliveryZone1 = CreateDeliveryZonePersistenceModel(
			code: "NORTH-001",
			name: "Northern Zone"
		);
		DeliveryZonePersistenceModel deliveryZone2 = CreateDeliveryZonePersistenceModel(
			code: "SOUTH-002",
			name: "Southern Zone"
		);
		DeliveryZonePersistenceModel deliveryZone3 = CreateDeliveryZonePersistenceModel(
			code: "EAST-003",
			name: "Eastern Zone"
		);

		await _dbContext.DeliveryZones.AddRangeAsync(deliveryZone1, deliveryZone2, deliveryZone3);
		await _dbContext.SaveChangesAsync();

		var query = new GetAllDeliveryZonesQuery();

		// Act
		Result<IReadOnlyList<DeliveryZoneSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(3);
		result.Value.Should().BeEquivalentTo([
			new DeliveryZoneSummaryDto(deliveryZone1.Id, "NORTH-001", "Northern Zone"),
			new DeliveryZoneSummaryDto(deliveryZone2.Id, "SOUTH-002", "Southern Zone"),
			new DeliveryZoneSummaryDto(deliveryZone3.Id, "EAST-003", "Eastern Zone")
		]);
	}
}
