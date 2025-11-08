using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Route.Errors;
using MsLogistic.Domain.Route.Events;
using MsLogistic.Domain.Route.Types;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Domain.Route.Entities;

public class Route : AggregateRoot
{
    public Guid DeliveryZoneId { get; private set; }
    public Guid? DeliveryPersonId { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public GeoPointValue OriginLocation { get; private set; }
    public RouteStatusType Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Route()
    {
    }

    public Route(
        Guid deliveryZoneId,
        DateTime scheduledDate,
        GeoPointValue originLocation) : base(Guid.NewGuid())
    {
        if (scheduledDate < DateTime.UtcNow.Date)
        {
            throw new DomainException(RouteErrors.ScheduleDateCannotBeInThePast);
        }

        DeliveryZoneId = deliveryZoneId;
        ScheduledDate = scheduledDate;
        OriginLocation = originLocation;
        Status = RouteStatusType.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void AssignDeliveryPerson(Guid deliveryPersonId)
    {
        if (Status != RouteStatusType.Pending)
        {
            throw new DomainException(RouteErrors.CannotChangeDeliveryPersonIfNotPending);
        }

        DeliveryPersonId = deliveryPersonId;
        MarkAsUpdated();
    }

    public void UnassignDeliveryPerson()
    {
        if (Status != RouteStatusType.Pending)
        {
            throw new DomainException(RouteErrors.CannotChangeDeliveryPersonIfNotPending);
        }

        DeliveryPersonId = null;
        MarkAsUpdated();
    }

    public void Start()
    {
        if (Status != RouteStatusType.ReadyToStart)
        {
            throw new DomainException(RouteErrors.CannotChangeStatusFromTo(Status, RouteStatusType.InProgress));
        }

        if (DeliveryPersonId == null)
        {
            throw new DomainException(RouteErrors.DeliveryPersonIsRequired);
        }

        Status = RouteStatusType.InProgress;
        StartedAt = DateTime.UtcNow;
        MarkAsUpdated();

        var domainEvent = new RouteStarted(Id, StartedAt.Value);
        AddDomainEvent(domainEvent);
    }

    public void Complete()
    {
        if (Status != RouteStatusType.InProgress)
        {
            throw new InvalidOperationException("Only in-progress routes can be completed.");
        }

        Status = RouteStatusType.Completed;
        CompletedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Cancel()
    {
        if (Status == RouteStatusType.Completed)
        {
            throw new InvalidOperationException("Completed routes cannot be canceled.");
        }

        Status = RouteStatusType.Cancelled;
        MarkAsUpdated();
    }
}