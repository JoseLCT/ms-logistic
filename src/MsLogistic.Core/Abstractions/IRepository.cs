namespace MsLogistic.Core.Abstractions;

public interface IRepository<TEntity> where TEntity : AggregateRoot
{
    Task<TEntity?> GetByIdAsync(Guid id, bool readOnly = false);
    Task AddAsync(TEntity entity);
}