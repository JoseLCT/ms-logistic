using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Drivers.UpdateDriver;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Drivers.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Application.Drivers;

public class UpdateDriverHandlerTest {
	private readonly Mock<IDriverRepository> _driverRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly UpdateDriverHandler _handler;

	public UpdateDriverHandlerTest() {
		_driverRepositoryMock = new Mock<IDriverRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<UpdateDriverHandler>>();

		_handler = new UpdateDriverHandler(
			_driverRepositoryMock.Object,
			_unitOfWorkMock.Object,
			loggerMock.Object
		);
	}

	[Fact]
	public async Task Handle_WithExistingDriver_ShouldUpdateAndReturnSuccess() {
		// Arrange
		var driver = Driver.Create("John Doe");
		var command = new UpdateDriverCommand(driver.Id, "Jane Doe", false);
		Driver? capturedDriver = null;

		_driverRepositoryMock
			.Setup(x => x.GetByIdAsync(driver.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(driver);

		_driverRepositoryMock
			.Setup(x => x.Update(It.IsAny<Driver>()))
			.Callback<Driver>(d => capturedDriver = d);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		capturedDriver.FullName.Should().Be(command.FullName);
		capturedDriver.IsActive.Should().Be(command.IsActive);
		_driverRepositoryMock.Verify(x => x.Update(driver), Times.Once);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WithNonExistingDriver_ShouldReturnNotFoundErrorAndNotUpdateOrCommit() {
		// Arrange
		var driverId = Guid.NewGuid();
		var command = new UpdateDriverCommand(driverId, "Jane Doe", true);

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
		_driverRepositoryMock.Verify(x => x.Update(It.IsAny<Driver>()), Times.Never);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WithEmptyFullName_ShouldThrowDomainExceptionAndNotUpdateOrCommit() {
		// Arrange
		var driver = Driver.Create("John Doe");
		var command = new UpdateDriverCommand(driver.Id, string.Empty, true);

		_driverRepositoryMock
			.Setup(x => x.GetByIdAsync(driver.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(driver);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DomainException>();
		_driverRepositoryMock.Verify(x => x.Update(It.IsAny<Driver>()), Times.Never);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenRepositoryThrowsException_ShouldNotCommit() {
		// Arrange
		var driverId = Guid.NewGuid();
		var command = new UpdateDriverCommand(driverId, "Jane Doe", true);

		_driverRepositoryMock
			.Setup(x => x.GetByIdAsync(driverId, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Repository error"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>();
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}
