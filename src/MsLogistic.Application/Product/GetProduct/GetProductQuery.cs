using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Product.GetProduct;

public record GetProductQuery(Guid Id) : IRequest<Result<ProductDetailDto>>;