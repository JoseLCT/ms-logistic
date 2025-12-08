using FluentAssertions;
using Moq;
using MsLogistic.Application.Customer.CreateCustomer;
using MsLogistic.Core.Abstractions;
using MsLogistic.Domain.Customer.Errors;
using MsLogistic.Domain.Customer.Repositories;
using Xunit;

namespace MsLogistic.Test.Application.Customer;

public class CreateCustomerHandlerTest
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateCustomerHandler _handler;

    public CreateCustomerHandlerTest()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateCustomerHandler(_customerRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateCustomerHandler_Handle_ShouldCreateCustomerSuccessfully()
    {
        // Arrange
        const string customerName = "John Doe";
        const string customerPhoneNumber = "1234567890";
        var request = new CreateCustomerCommand(customerName, customerPhoneNumber);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _customerRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<MsLogistic.Domain.Customer.Entities.Customer>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateCustomerHandler_Handle_ShouldFailWhenNameIsEmpty()
    {
        // Arrange
        const string customerName = "";
        const string customerPhoneNumber = "1234567890";
        var request = new CreateCustomerCommand(customerName, customerPhoneNumber);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(CustomerErrors.NameIsRequired);
    }
}