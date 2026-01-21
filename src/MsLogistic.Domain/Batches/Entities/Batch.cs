using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Batches.Enums;
using MsLogistic.Domain.Batches.Errors;
using MsLogistic.Domain.Batches.Events;

namespace MsLogistic.Domain.Batches.Entities;

public class Batch : AggregateRoot
{
    public int TotalOrders { get; private set; }
    public BatchStatusEnum Status { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    private Batch()
    {
    }

    private Batch(int totalOrders)
        : base(Guid.NewGuid())
    {
        TotalOrders = totalOrders;
        Status = BatchStatusEnum.Open;
        OpenedAt = DateTime.UtcNow;
    }

    public static Batch Create(int totalOrders = 0)
    {
        if (totalOrders < 0)
        {
            throw new DomainException(BatchErrors.TotalOrdersCannotBeNegative);
        }

        return new Batch(totalOrders);
    }

    public void AddOrders(int quantity)
    {
        if (Status != BatchStatusEnum.Open)
        {
            throw new DomainException(BatchErrors.CannotAddOrdersToClosedBatch);
        }

        if (quantity <= 0)
        {
            throw new DomainException(BatchErrors.CannotAddNonPositiveQuantityOfOrders);
        }

        TotalOrders += quantity;
        MarkAsUpdated();
    }

    public void Close()
    {
        if (Status != BatchStatusEnum.Open)
        {
            throw new DomainException(BatchErrors.CannotCloseAlreadyClosedBatch);
        }

        Status = BatchStatusEnum.Closed;
        ClosedAt = DateTime.UtcNow;

        var domainEvent = new BatchClosed(Id);
        AddDomainEvent(domainEvent);
        MarkAsUpdated();
    }
}