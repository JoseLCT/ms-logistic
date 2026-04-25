using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.DeliveryZones.RemoveDeliveryZone;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryZones.Entities;
using MsLogistic.Domain.DeliveryZones.Repositories;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Application.DeliveryZones;

public class RemoveDeliveryZoneHandlerTest {
	private readonly Mock<IDeliveryZoneRepository> _deliveryZoneRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly RemoveDeliveryZoneHandler _handler;

	public RemoveDeliveryZoneHandlerTest() {
		_deliveryZoneRepositoryMock = new Mock<IDeliveryZoneRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<RemoveDeliveryZoneHandler>>();

		_handler = new RemoveDeliveryZoneHandler(
			_deliveryZoneRepositoryMock.Object,
			_unitOfWorkMock.Object,
			loggerMock.Object
		);
	}

	private static DeliveryZone CreateValidDeliveryZone() {
		var geoPoints = new List<GeoPointValue> {
			GeoPointValue.Create(-17.7833, -63.1821),
			GeoPointValue.Create(-17.7840, -63.1830),
			GeoPointValue.Create(-17.7850, -63.1810),
			GeoPointValue.Create(-17.7833, -63.1821)
		};

		var boundaries = BoundariesValue.Create(geoPoints);

		return DeliveryZone.Create(
			driverId: null,
			code: "ZON-001",
			name: "Zona Norte",
			boundaries: boundaries
		);
	}

	[Fact]
	public async Task Handle_WhenDeliveryZoneExists_ShouldRemoveAndCommit() {
		// Arrange
		DeliveryZone deliveryZone = CreateValidDeliveryZone();
		var command = new RemoveDeliveryZoneCommand(deliveryZone.Id);

		_deliveryZoneRepositoryMock
			.Setup(r => r.GetByIdAsync(deliveryZone.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(deliveryZone);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		_deliveryZoneRepositoryMock.Verify(r => r.Remove(deliveryZone), Times.Once);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenDeliveryZoneDoesNotExist_ShouldReturnFailure() {
		// Arrange
		var deliveryZoneId = Guid.NewGuid();
		var command = new RemoveDeliveryZoneCommand(deliveryZoneId);

		_deliveryZoneRepositoryMock
			.Setup(r => r.GetByIdAsync(deliveryZoneId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((DeliveryZone?)null);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("DeliveryZone", deliveryZoneId));

		_deliveryZoneRepositoryMock.Verify(r => r.Remove(It.IsAny<DeliveryZone>()), Times.Never);
		_unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}
