using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Product.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<Result<Guid>>;