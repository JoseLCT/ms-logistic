using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Routes.Enums;
using MsLogistic.Domain.Routes.Errors;
using MsLogistic.Domain.Routes.Events;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Domain.Routes.Entities;

public class Route : AggregateRoot
{
    public Guid BatchId { get; private set; }
    public Guid DeliveryZoneId { get; private set; }
    public Guid? DriverId { get; private set; }
    public GeoPointValue OriginLocation { get; private set; }
    public RouteStatusEnum Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Route()
    {
    }

    private Route(
        Guid batchId,
        Guid deliveryZoneId,
        Guid? driverId,
        GeoPointValue originLocation
    ) : base(Guid.NewGuid())
    {
        BatchId = batchId;
        DeliveryZoneId = deliveryZoneId;
        DriverId = driverId;
        OriginLocation = originLocation;
        Status = RouteStatusEnum.Pending;
    }

    public static Route Create(
        Guid batchId,
        Guid deliveryZoneId,
        Guid? driverId,
        GeoPointValue originLocation
    )
    {
        return new Route(
            batchId,
            deliveryZoneId,
            driverId,
            originLocation
        );
    }

    public void AssignDriver(Guid driverId)
    {
        if (Status != RouteStatusEnum.Pending)
        {
            throw new DomainException(RouteErrors.CannotChangeDriverIfNotPending);
        }

        DriverId = driverId;
        MarkAsUpdated();
    }

    public void UnassignDriver()
    {
        if (Status != RouteStatusEnum.Pending)
        {
            throw new DomainException(RouteErrors.CannotChangeDriverIfNotPending);
        }

        DriverId = null;
        MarkAsUpdated();
    }

    public void Start()
    {
        if (Status != RouteStatusEnum.Pending)
        {
            throw new DomainException(RouteErrors.CannotChangeStatusFromTo(Status, RouteStatusEnum.InProgress));
        }

        if (DriverId == null)
        {
            throw new DomainException(RouteErrors.DriverIsRequired);
        }

        Status = RouteStatusEnum.InProgress;
        StartedAt = DateTime.UtcNow;

        var domainEvent = new RouteStarted(Id, StartedAt.Value);
        AddDomainEvent(domainEvent);
        MarkAsUpdated();
    }

    public void Complete()
    {
        if (Status != RouteStatusEnum.InProgress)
        {
            throw new DomainException(RouteErrors.CannotChangeStatusFromTo(Status, RouteStatusEnum.Completed));
        }

        Status = RouteStatusEnum.Completed;
        CompletedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Cancel()
    {
        if (Status == RouteStatusEnum.Completed)
        {
            throw new DomainException(RouteErrors.CannotChangeStatusFromTo(Status, RouteStatusEnum.Cancelled));
        }

        Status = RouteStatusEnum.Cancelled;

        var domainEvent = new RouteCancelled(Id, DateTime.UtcNow);
        AddDomainEvent(domainEvent);
        MarkAsUpdated();
    }
}