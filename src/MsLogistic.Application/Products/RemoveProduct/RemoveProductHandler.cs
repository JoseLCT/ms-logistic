using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Repositories;
using MsLogistic.Domain.Shared.Errors;

namespace MsLogistic.Application.Products.RemoveProduct;

public class RemoveProductHandler : IRequestHandler<RemoveProductCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveProductHandler> _logger;

    public RemoveProductHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<RemoveProductHandler> logger
    )
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RemoveProductCommand request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, ct);

        if (product is null)
        {
            return Result.Failure<Guid>(
                CommonErrors.NotFoundById("Product", request.Id)
            );
        }

        _productRepository.Remove(product);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Product with id {ProductId} removed successfully.", product.Id);

        return Result.Success(product.Id);
    }
}