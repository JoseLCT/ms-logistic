using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Customers.GetCustomerById;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Customers;

internal class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDetailDto>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetCustomerByIdHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CustomerDetailDto>> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new CustomerDetailDto(
                c.Id,
                c.FullName,
                c.PhoneNumber
            ))
            .FirstOrDefaultAsync(ct);

        if (customer == null)
        {
            return Result.Failure<CustomerDetailDto>(
                CommonErrors.NotFoundById("Customer", request.Id)
            );
        }

        return Result.Success(customer);
    }
}