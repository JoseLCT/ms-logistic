using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Domain.Orders.Errors;
using MsLogistic.Domain.Orders.Events;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Domain.Orders.Entities;

public class Order : AggregateRoot
{
    public Guid BatchId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? RouteId { get; private set; }
    public int? DeliverySequence { get; private set; }
    public OrderStatusEnum Status { get; private set; }
    public DateTime ScheduledDeliveryDate { get; private set; }
    public string DeliveryAddress { get; private set; }
    public GeoPointValue DeliveryLocation { get; private set; }
    public OrderDelivery? Delivery { get; private set; }
    public OrderIncident? Incident { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items;

    private Order()
    {
    }

    private Order(
        Guid batchId,
        Guid customerId,
        DateTime scheduledDeliveryDate,
        string deliveryAddress,
        GeoPointValue deliveryLocation
    ) : base(Guid.NewGuid())
    {
        BatchId = batchId;
        CustomerId = customerId;
        Status = OrderStatusEnum.Pending;
        ScheduledDeliveryDate = scheduledDeliveryDate;
        DeliveryAddress = deliveryAddress;
        DeliveryLocation = deliveryLocation;
    }

    public static Order Create(
        Guid batchId,
        Guid customerId,
        DateTime scheduledDeliveryDate,
        string deliveryAddress,
        GeoPointValue deliveryLocation
    )
    {
        if (scheduledDeliveryDate < DateTime.UtcNow.Date)
        {
            throw new DomainException(OrderErrors.ScheduledDeliveryDateCannotBeInThePast);
        }

        if (string.IsNullOrWhiteSpace(deliveryAddress))
        {
            throw new DomainException(OrderErrors.DeliveryAddressIsRequired);
        }

        return new Order(
            batchId,
            customerId,
            scheduledDeliveryDate,
            deliveryAddress,
            deliveryLocation
        );
    }

    public void AddItem(Guid productId, int quantity)
    {
        if (Status != OrderStatusEnum.Pending)
        {
            throw new DomainException(OrderErrors.CannotModifyOrderThatIsNotPending);
        }

        var item = OrderItem.Create(Id, productId, quantity);

        var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(item);
        }

        MarkAsUpdated();
    }

    public void AssignToRoute(Guid routeId, int deliverySequence)
    {
        if (Status != OrderStatusEnum.Pending)
        {
            throw new DomainException(OrderErrors.CannotAssignOrderThatIsNotPending);
        }

        if (deliverySequence <= 0)
        {
            throw new DomainException(OrderErrors.DeliverySequenceMustBeGreaterThanZero);
        }

        if (_items.Count == 0)
        {
            throw new DomainException(OrderErrors.CannotAssignOrderWithoutItems);
        }

        RouteId = routeId;
        DeliverySequence = deliverySequence;
        MarkAsUpdated();
    }

    public void MarkAsInTransit()
    {
        if (Status != OrderStatusEnum.Pending)
        {
            throw new DomainException(OrderErrors.CannotChangeStatusFromTo(Status, OrderStatusEnum.InTransit));
        }

        Status = OrderStatusEnum.InTransit;
        MarkAsUpdated();
    }

    public void Cancel()
    {
        if (Status != OrderStatusEnum.Pending && Status != OrderStatusEnum.InTransit)
        {
            throw new DomainException(OrderErrors.CannotChangeStatusFromTo(Status, OrderStatusEnum.Cancelled));
        }

        Status = OrderStatusEnum.Cancelled;

        var domainEvent = new OrderCancelled(Id, DateTime.UtcNow);
        AddDomainEvent(domainEvent);
        MarkAsUpdated();
    }

    public void ReportIncident(
        Guid driverId,
        OrderIncidentTypeEnum incidentType,
        string description
    )
    {
        if (Incident != null)
        {
            throw new DomainException(OrderErrors.IncidentAlreadyReported);
        }

        if (Status != OrderStatusEnum.InTransit)
        {
            throw new DomainException(OrderErrors.CannotReportIncidentForOrderWithStatus(Status));
        }

        var incident = OrderIncident.Create(Id, driverId, incidentType, description);

        Incident = incident;
        Status = OrderStatusEnum.Failed;

        var domainEvent = new OrderIncidentReported(Id, incidentType, DateTime.UtcNow);
        AddDomainEvent(domainEvent);
        MarkAsUpdated();
    }

    public void Deliver(
        Guid driverId,
        GeoPointValue location,
        string? comments,
        string? imageUrl
    )
    {
        if (Status != OrderStatusEnum.InTransit)
        {
            throw new DomainException(OrderErrors.CannotDeliverOrderWithStatus(Status));
        }

        var delivery = OrderDelivery.Create(Id, driverId, location, DateTime.UtcNow, comments, imageUrl);

        Delivery = delivery;
        Status = OrderStatusEnum.Delivered;

        var domainEvent = new OrderDelivered(Id, DateTime.UtcNow);
        AddDomainEvent(domainEvent);
        MarkAsUpdated();
    }
}