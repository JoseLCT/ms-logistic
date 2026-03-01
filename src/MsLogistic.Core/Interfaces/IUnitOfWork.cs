namespace MsLogistic.Core.Interfaces;

public interface IUnitOfWork {
    Task CommitAsync(CancellationToken ct = default);
}
