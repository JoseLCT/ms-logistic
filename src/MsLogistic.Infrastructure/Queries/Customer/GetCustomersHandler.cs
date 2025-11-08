using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Customer.GetCustomers;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Customer;

internal class GetCustomersHandler : IRequestHandler<GetCustomersQuery, Result<ICollection<CustomerSummaryDto>>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetCustomersHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ICollection<CustomerSummaryDto>>> Handle(GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var customers = await _dbContext.Customer
            .AsNoTracking()
            .Select(c => new CustomerSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber
            })
            .ToListAsync(cancellationToken);

        return customers;
    }
}