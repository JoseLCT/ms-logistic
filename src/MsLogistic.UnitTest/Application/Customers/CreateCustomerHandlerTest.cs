using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Customers.CreateCustomer;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Customers.Errors;
using MsLogistic.Domain.Customers.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Application.Customers;

public class CreateCustomerHandlerTest
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateCustomerHandler>> _loggerMock;
    private readonly CreateCustomerHandler _handler;

    public CreateCustomerHandlerTest()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateCustomerHandler>>();
        _handler = new CreateCustomerHandler(_customerRepositoryMock.Object, _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCustomerAndReturnSuccessResult()
    {
        // Arrange
        const string fullName = "Juan Perez";
        const string phoneNumber = "+591 12345678";
        var command = new CreateCustomerCommand(fullName, phoneNumber);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _customerRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithEmptyFullName_ShouldReturnFailureResult()
    {
        // Arrange
        const string fullName = "";
        const string phoneNumber = "+591 12345678";
        var command = new CreateCustomerCommand(fullName, phoneNumber);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Error == CustomerErrors.FullNameIsRequired);
    }
}