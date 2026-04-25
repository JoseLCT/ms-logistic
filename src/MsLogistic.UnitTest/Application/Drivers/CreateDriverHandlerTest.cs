using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Drivers.CreateDriver;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Drivers.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Application.Drivers;

public class CreateDriverHandlerTest {
	private readonly Mock<IDriverRepository> _driverRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly CreateDriverHandler _handler;

	public CreateDriverHandlerTest() {
		_driverRepositoryMock = new Mock<IDriverRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<CreateDriverHandler>>();

		_handler = new CreateDriverHandler(
			_driverRepositoryMock.Object,
			_unitOfWorkMock.Object,
			loggerMock.Object
		);
	}

	[Fact]
	public async Task Handle_WithValidCommand_ShouldCreateDriverAndReturnSuccess() {
		// Arrange
		var command = new CreateDriverCommand("John Doe");
		Driver? capturedDriver = null;

		_driverRepositoryMock
			.Setup(x => x.AddAsync(It.IsAny<Driver>(), It.IsAny<CancellationToken>()))
			.Callback<Driver, CancellationToken>((driver, _) => capturedDriver = driver)
			.Returns(Task.CompletedTask);

		// Act
		Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Error.Should().BeNull();
		capturedDriver.Should().NotBeNull();
		capturedDriver.FullName.Should().Be(command.FullName);
		result.Value.Should().Be(capturedDriver.Id);
		_driverRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Driver>(), It.IsAny<CancellationToken>()), Times.Once);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WithEmptyFullName_ShouldThrowDomainExceptionAndNotAddOrCommit() {
		// Arrange
		var command = new CreateDriverCommand(string.Empty);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DomainException>();
		_driverRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Driver>(), It.IsAny<CancellationToken>()), Times.Never);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}
