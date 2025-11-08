using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Product.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description
) : IRequest<Result<Guid>>;