using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Product.GetProducts;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Product;

internal class GetProductsHandler : IRequestHandler<GetProductsQuery, Result<ICollection<ProductSummaryDto>>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetProductsHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ICollection<ProductSummaryDto>>> Handle(GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _dbContext.Product
            .AsNoTracking()
            .Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
            })
            .ToListAsync(cancellationToken);

        return products;
    }
}