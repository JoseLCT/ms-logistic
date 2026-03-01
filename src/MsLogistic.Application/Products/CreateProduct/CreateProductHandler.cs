using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Repositories;

namespace MsLogistic.Application.Products.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<Guid>> {
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateProductHandler> logger
    ) {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct) {
        var product = Product.Create(request.Name, request.Description);

        await _productRepository.AddAsync(product, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Product with id {ProductId} created successfully.", product.Id);

        return Result.Success(product.Id);
    }
}
