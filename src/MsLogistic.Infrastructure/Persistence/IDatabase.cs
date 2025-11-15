namespace MsLogistic.Infrastructure.Persistence;

public interface IDatabase : IDisposable
{
    void Migrate();
}