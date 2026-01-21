using MediatR;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Products.GetProductById;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;

namespace MsLogistic.Infrastructure.Queries.Products;

internal class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDetailDto>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetProductByIdHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProductDetailDto>> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductDetailDto(
                p.Id,
                p.Name,
                p.Description
            ))
            .FirstOrDefaultAsync(ct);

        if (product is null)
        {
            return Result.Failure<ProductDetailDto>(
                CommonErrors.NotFoundById("Product", request.Id)
            );
        }

        return Result.Success(product);
    }
}