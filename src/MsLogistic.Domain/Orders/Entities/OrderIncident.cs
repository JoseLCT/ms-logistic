using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Domain.Orders.Errors;

namespace MsLogistic.Domain.Orders.Entities;

public class OrderIncident : Entity
{
    public Guid OrderId { get; private set; }
    public Guid DriverId { get; private set; }
    public OrderIncidentTypeEnum IncidentType { get; private set; }
    public string Description { get; private set; }

    private OrderIncident()
    {
    }

    private OrderIncident(
        Guid orderId,
        Guid driverId,
        OrderIncidentTypeEnum incidentType,
        string description
    ) : base(Guid.NewGuid())
    {
        OrderId = orderId;
        DriverId = driverId;
        IncidentType = incidentType;
        Description = description;
    }

    public static OrderIncident Create(
        Guid orderId,
        Guid driverId,
        OrderIncidentTypeEnum incidentType,
        string description
    )
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException(OrderIncidentErrors.DescriptionIsRequired);
        }

        return new OrderIncident(
            orderId,
            driverId,
            incidentType,
            description
        );
    }
}