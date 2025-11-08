using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Order.Errors;
using MsLogistic.Domain.Order.Events;
using MsLogistic.Domain.Order.Types;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Domain.Order.Entities;

public class Order : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public Guid? RouteId { get; private set; }
    public int? DeliverySequence { get; private set; }
    public OrderStatusType Status { get; private set; }
    public DateTime ScheduledDeliveryDate { get; private set; }
    public string DeliveryAddress { get; private set; }
    public GeoPointValue DeliveryLocation { get; private set; }
    private readonly List<OrderItem> _items;
    public IReadOnlyCollection<OrderItem> Items => _items;
    public OrderDelivery? Delivery { get; private set; }

    private Order()
    {
    }

    public Order(
        Guid customerId,
        DateTime scheduledDeliveryDate,
        string deliveryAddress,
        GeoPointValue deliveryLocation) : base(Guid.NewGuid())
    {
        if (scheduledDeliveryDate < DateTime.UtcNow.Date)
        {
            throw new DomainException(OrderErrors.ScheduledDeliveryDateCannotBeInThePast);
        }

        if (string.IsNullOrWhiteSpace(deliveryAddress))
        {
            throw new DomainException(OrderErrors.DeliveryAddressIsRequired);
        }

        CustomerId = customerId;
        Status = OrderStatusType.Pending;
        ScheduledDeliveryDate = scheduledDeliveryDate;
        DeliveryAddress = deliveryAddress;
        DeliveryLocation = deliveryLocation;
        _items = [];
        CreatedAt = DateTime.UtcNow;
    }

    public void SetCustomerId(Guid customerId)
    {
        if (Status != OrderStatusType.Pending)
        {
            throw new DomainException(OrderErrors.CannotModifyOrderThatIsNotPending);
        }

        CustomerId = customerId;
        MarkAsUpdated();
    }

    public void SetScheduledDeliveryDate(DateTime scheduledDeliveryDate)
    {
        if (Status != OrderStatusType.Pending)
        {
            throw new DomainException(OrderErrors.CannotModifyOrderThatIsNotPending);
        }

        if (scheduledDeliveryDate < DateTime.UtcNow.Date)
        {
            throw new DomainException(OrderErrors.ScheduledDeliveryDateCannotBeInThePast);
        }

        ScheduledDeliveryDate = scheduledDeliveryDate;
        MarkAsUpdated();
    }

    public void SetDeliveryAddress(string deliveryAddress)
    {
        if (Status != OrderStatusType.Pending)
        {
            throw new DomainException(OrderErrors.CannotModifyOrderThatIsNotPending);
        }

        if (string.IsNullOrWhiteSpace(deliveryAddress))
        {
            throw new DomainException(OrderErrors.DeliveryAddressIsRequired);
        }

        DeliveryAddress = deliveryAddress;
        MarkAsUpdated();
    }

    public void SetDeliveryLocation(GeoPointValue deliveryLocation)
    {
        if (Status != OrderStatusType.Pending)
        {
            throw new DomainException(OrderErrors.CannotModifyOrderThatIsNotPending);
        }

        DeliveryLocation = deliveryLocation;
        MarkAsUpdated();
    }

    public void AddItem(Guid productId, int quantity)
    {
        if (Status != OrderStatusType.Pending)
        {
            throw new DomainException(OrderErrors.CannotModifyOrderThatIsNotPending);
        }

        if (quantity <= 0)
        {
            throw new DomainException(OrderItemErrors.QuantityMustBeGreaterThanZero);
        }

        var item = new OrderItem(Id, productId, quantity);
        _items.Add(item);
        MarkAsUpdated();
    }

    public void AssignToRoute(Guid routeId, int deliverySequence)
    {
        if (Status != OrderStatusType.Pending)
        {
            throw new DomainException(OrderErrors.CannotChangeStatusFromTo(Status, OrderStatusType.Pending));
        }

        if (deliverySequence <= 0)
        {
            throw new DomainException(OrderErrors.DeliverySequenceMustBeGreaterThanZero);
        }

        if (_items.Count == 0)
        {
            throw new DomainException(OrderErrors.CannotAssignOrderWithNoItems);
        }

        RouteId = routeId;
        DeliverySequence = deliverySequence;
        MarkAsUpdated();
    }

    public void UnassignFromRoute()
    {
        if (!RouteId.HasValue)
        {
            return;
        }

        if (Status != OrderStatusType.Pending)
        {
            throw new DomainException(OrderErrors.CannotChangeStatusFromTo(Status, OrderStatusType.Pending));
        }

        RouteId = null;
        DeliverySequence = null;
        MarkAsUpdated();
    }

    public void MarkAsInProgress()
    {
        if (Status != OrderStatusType.Pending)
        {
            throw new DomainException(OrderErrors.CannotChangeStatusFromTo(Status, OrderStatusType.InProgress));
        }

        Status = OrderStatusType.InProgress;
        MarkAsUpdated();
    }

    public void Cancel()
    {
        if (Status == OrderStatusType.Completed)
        {
            throw new DomainException(OrderErrors.CannotChangeStatusFromTo(Status, OrderStatusType.Cancelled));
        }

        Status = OrderStatusType.Cancelled;
        MarkAsUpdated();

        var domainEvent = new OrderCancelled(Id);
        AddDomainEvent(domainEvent);
    }

    public void Deliver(
        Guid deliveryPersonId,
        GeoPointValue deliveryLocation,
        string? comments)
    {
        if (Status != OrderStatusType.InProgress)
        {
            throw new DomainException(OrderErrors.CannotChangeStatusFromTo(Status, OrderStatusType.Completed));
        }

        Delivery = new OrderDelivery(Id, deliveryPersonId, deliveryLocation, DateTime.UtcNow, comments);
        Status = OrderStatusType.Completed;
        MarkAsUpdated();

        var domainEvent = new OrderCompleted(Id, DateTime.UtcNow);
        AddDomainEvent(domainEvent);
    }
}