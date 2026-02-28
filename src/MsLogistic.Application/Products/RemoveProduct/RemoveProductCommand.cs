using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Products.RemoveProduct;

public record RemoveProductCommand(Guid Id) : IRequest<Result<Guid>>;
