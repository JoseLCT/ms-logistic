using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Products.GetAllProducts;

public record GetAllProductsQuery() : IRequest<Result<IReadOnlyList<ProductSummaryDto>>>;