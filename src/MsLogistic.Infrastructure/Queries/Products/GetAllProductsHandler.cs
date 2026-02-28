using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Products.GetAllProducts;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Products;

internal class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, Result<IReadOnlyList<ProductSummaryDto>>> {
    private readonly PersistenceDbContext _dbContext;

    public GetAllProductsHandler(PersistenceDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<ProductSummaryDto>>> Handle(
        GetAllProductsQuery request,
        CancellationToken ct
    ) {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Select(p => new ProductSummaryDto(
                p.Id,
                p.Name
            ))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<ProductSummaryDto>>(products);
    }
}
