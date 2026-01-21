using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Products.UpdateProduct;

public record UpdateProductCommand(Guid Id, string Name, string? Description) : IRequest<Result<Guid>>;