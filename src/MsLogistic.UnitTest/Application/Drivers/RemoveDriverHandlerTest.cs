using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Drivers.RemoveDriver;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Drivers.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Application.Drivers;

public class RemoveDriverHandlerTest {
	private readonly Mock<IDriverRepository> _driverRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly RemoveDriverHandler _handler;

	public RemoveDriverHandlerTest() {
		_driverRepositoryMock = new Mock<IDriverRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<RemoveDriverHandler>>();

		_handler = new RemoveDriverHandler(
			_driverRepositoryMock.Object,
			_unitOfWorkMock.Object,
			loggerMock.Object
		);
	}

	[Fact]
	public async Task Handle_WithExistingDriver_ShouldRemoveDriverAndReturnSuccess() {
		// Arrange
		var driver = Driver.Create("John Doe");
		var command = new RemoveDriverCommand(driver.Id);

		_driverRepositoryMock
			.Setup(x => x.GetByIdAsync(driver.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(driver);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		_driverRepositoryMock.Verify(x => x.Remove(driver), Times.Once);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WithNonExistingDriver_ShouldReturnNotFoundErrorAndNotRemoveOrCommit() {
		// Arrange
		var driverId = Guid.NewGuid();
		var command = new RemoveDriverCommand(driverId);

		_driverRepositoryMock
			.Setup(x => x.GetByIdAsync(driverId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Driver?)null);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Error.Should().NotBeNull();
		result.Error.Type.Should().Be(ErrorType.NotFound);
		result.Error.Code.Should().Contain("Driver");
		result.Error.Message.Should().Contain(driverId.ToString());
		_driverRepositoryMock.Verify(x => x.Remove(It.IsAny<Driver>()), Times.Never);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}
