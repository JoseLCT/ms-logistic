using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Customers.GetAllCustomers;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Customers;

internal class GetAllCustomersHandler : IRequestHandler<GetAllCustomersQuery, Result<IReadOnlyList<CustomerSummaryDto>>> {
    private readonly PersistenceDbContext _dbContext;

    public GetAllCustomersHandler(PersistenceDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<CustomerSummaryDto>>> Handle(
        GetAllCustomersQuery request,
        CancellationToken ct
    ) {
        var customers = await _dbContext.Customers
            .AsNoTracking()
            .Select(c => new CustomerSummaryDto(
                c.Id,
                c.FullName
            ))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<CustomerSummaryDto>>(customers);
    }
}
