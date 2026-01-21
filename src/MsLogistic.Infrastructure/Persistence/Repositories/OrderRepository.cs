using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Infrastructure.Persistence.DomainModel;

namespace MsLogistic.Infrastructure.Persistence.Repositories;

internal class OrderRepository : IOrderRepository
{
    private readonly DomainDbContext _dbContext;

    public OrderRepository(DomainDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
    {
        var orders = await _dbContext.Orders.ToListAsync(ct);
        return orders;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        return order;
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _dbContext.Orders.AddAsync(order, ct);
    }

    public void Update(Order order)
    {
        _dbContext.Orders.Update(order);
    }

    public void Remove(Order order)
    {
        _dbContext.Orders.Remove(order);
    }
}