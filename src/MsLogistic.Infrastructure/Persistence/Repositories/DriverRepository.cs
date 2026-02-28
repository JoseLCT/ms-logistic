using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Drivers.Repositories;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class DriverRepository : IDriverRepository {
    private readonly DomainDbContext _dbContext;

    public DriverRepository(DomainDbContext dbContext) {
        _dbContext = dbContext;
    }


    public async Task<IReadOnlyList<Driver>> GetAllAsync(CancellationToken ct = default) {
        var drivers = await _dbContext.Drivers.ToListAsync(ct);
        return drivers;
    }

    public async Task<Driver?> GetByIdAsync(Guid id, CancellationToken ct = default) {
        var driver = await _dbContext.Drivers.FirstOrDefaultAsync(d => d.Id == id, ct);
        return driver;
    }

    public async Task AddAsync(Driver driver, CancellationToken ct = default) {
        await _dbContext.Drivers.AddAsync(driver, ct);
    }

    public void Update(Driver driver) {
        _dbContext.Drivers.Update(driver);
    }

    public void Remove(Driver driver) {
        _dbContext.Drivers.Remove(driver);
    }
}
