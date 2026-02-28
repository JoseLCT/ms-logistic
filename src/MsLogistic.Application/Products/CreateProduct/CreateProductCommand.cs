using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Products.CreateProduct;

public record CreateProductCommand(string Name, string? Description) : IRequest<Result<Guid>>;
