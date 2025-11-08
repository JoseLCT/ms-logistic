using MediatR;

namespace MsLogistic.Core.Abstractions;

public abstract record DomainEvent : INotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccuredOn { get; set; } = DateTime.UtcNow;
}