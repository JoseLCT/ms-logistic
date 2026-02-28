using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Products.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDetailDto>>;
