using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Product.GetProduct;
using MsLogistic.Core.Results;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Product;

internal class GetProductHandler : IRequestHandler<GetProductQuery, Result<ProductDetailDto>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetProductHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProductDetailDto>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var productDto = await _dbContext.Product
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductDetailDto()
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (productDto == null)
        {
            return Result.Failure<ProductDetailDto>(
                Error.NotFound(
                    code: "product_not_found",
                    structuredMessage: $"Product with id {request.Id} was not found."
                )
            );
        }

        return Result.Success(productDto);
    }
}