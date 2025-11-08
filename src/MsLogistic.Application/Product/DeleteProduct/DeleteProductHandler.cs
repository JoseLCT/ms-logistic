using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Product.Repositories;

namespace MsLogistic.Application.Product.DeleteProduct;

internal class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);
        if (product is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound(
                    code: "product_not_found",
                    structuredMessage: $"Product with id {request.Id} was not found."
                )
            );
        }

        await _productRepository.DeleteAsync(product.Id);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}