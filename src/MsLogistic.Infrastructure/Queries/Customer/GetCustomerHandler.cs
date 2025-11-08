using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Customer.GetCustomer;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Customer;

internal class GetCustomerHandler : IRequestHandler<GetCustomerQuery, Result<CustomerDetailDto>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetCustomerHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CustomerDetailDto>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var customerDto = await _dbContext.Customer
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new CustomerDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (customerDto is null)
        {
            return Result.Failure<CustomerDetailDto>(
                Error.NotFound(
                    code: "customer_not_found",
                    structuredMessage: $"Customer with id {request.Id} was not found."
                )
            );
        }

        return Result.Success(customerDto);
    }
}