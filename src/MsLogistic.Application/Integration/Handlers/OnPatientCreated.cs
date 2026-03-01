using Joselct.Communication.Contracts.Services;
using MediatR;
using MsLogistic.Application.Customers.CreateCustomer;
using MsLogistic.Application.Integration.Events.Incoming;

namespace MsLogistic.Application.Integration.Handlers;

public class OnPatientCreated : IIntegrationMessageConsumer<PatientCreatedMessage> {
    private readonly IMediator _mediator;

    public OnPatientCreated(IMediator mediator) {
        _mediator = mediator;
    }

    public async Task HandleAsync(PatientCreatedMessage message, CancellationToken ct = default) {
        var command = new CreateCustomerCommand(
            FullName: message.FullName,
            PhoneNumber: message.PhoneNumber
        );

        await _mediator.Send(command, ct);
    }
}
