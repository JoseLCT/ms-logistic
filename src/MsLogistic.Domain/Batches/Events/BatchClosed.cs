using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Batches.Events;

public record BatchClosed(Guid BatchId) : DomainEvent;