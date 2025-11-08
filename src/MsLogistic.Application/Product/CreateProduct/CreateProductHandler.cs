using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Product.Repositories;

namespace MsLogistic.Application.Product.CreateProduct;

internal class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Domain.Product.Entities.Product(request.Name, request.Description);

        await _productRepository.AddAsync(product);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}