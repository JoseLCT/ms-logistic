using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Product.GetProducts;

public record GetProductsQuery() : IRequest<Result<ICollection<ProductSummaryDto>>>;