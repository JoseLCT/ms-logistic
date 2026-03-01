using Joselct.Outbox.Core.Interfaces;
using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Interfaces;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence;

internal class UnitOfWork : IUnitOfWork, IOutboxDatabase {
    private readonly DomainDbContext _dbContext;
    private readonly IMediator _mediator;

    public UnitOfWork(DomainDbContext dbContext, IMediator mediator) {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task CommitAsync(CancellationToken ct = default) {
        var domainEvents = _dbContext.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => {
                var events = x.Entity.DomainEvents.ToArray();
                x.Entity.ClearDomainEvents();
                return events;
            })
            .SelectMany(events => events)
            .ToList();

        foreach (var domainEvent in domainEvents) {
            await _mediator.Publish(domainEvent, ct);
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
