namespace MsLogistic.Core.Abstractions;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}