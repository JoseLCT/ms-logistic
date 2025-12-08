using FluentAssertions;
using Moq;
using MsLogistic.Application.DeliveryPerson.CreateDeliveryPerson;
using MsLogistic.Core.Abstractions;
using MsLogistic.Domain.DeliveryPerson.Repositories;
using Xunit;

namespace MsLogistic.Test.Application.DeliveryPerson;

public class CreateDeliveryPersonHandlerTest
{
    private readonly Mock<IDeliveryPersonRepository> _deliveryPersonRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateDeliveryPersonHandler _handler;

    public CreateDeliveryPersonHandlerTest()
    {
        _deliveryPersonRepositoryMock = new Mock<IDeliveryPersonRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateDeliveryPersonHandler(_deliveryPersonRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateDeliveryPersonHandler_Handle_ShouldCreateDeliveryPersonSuccessfully()
    {
        // Arrange
        const string deliveryPersonName = "Jane Smith";
        var request = new CreateDeliveryPersonCommand(deliveryPersonName);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _deliveryPersonRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<MsLogistic.Domain.DeliveryPerson.Entities.DeliveryPerson>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(cancellationToken),
            Times.Once
        );
    }
}