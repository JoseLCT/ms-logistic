using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Repositories;
using MsLogistic.Domain.Shared.Errors;

namespace MsLogistic.Application.Products.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result<Guid>> {
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductHandler> _logger;

    public UpdateProductHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProductHandler> logger
    ) {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UpdateProductCommand request, CancellationToken ct) {
        var product = await _productRepository.GetByIdAsync(request.Id, ct);

        if (product is null) {
            return Result.Failure<Guid>(
                CommonErrors.NotFoundById("Product", request.Id)
            );
        }

        product.SetName(request.Name);
        product.SetDescription(request.Description);

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Product with id {ProductId} updated successfully.", product.Id);

        return Result.Success(product.Id);
    }
}
