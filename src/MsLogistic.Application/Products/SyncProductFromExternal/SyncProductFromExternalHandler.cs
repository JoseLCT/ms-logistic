using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Repositories;

namespace MsLogistic.Application.Products.SyncProductFromExternal;

public class SyncProductFromExternalHandler : IRequestHandler<SyncProductFromExternalCommand, Result<Guid>> {
	private readonly IProductRepository _productRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<SyncProductFromExternalHandler> _logger;

	public SyncProductFromExternalHandler(
		IProductRepository productRepository,
		IUnitOfWork unitOfWork,
		ILogger<SyncProductFromExternalHandler> logger
	) {
		_productRepository = productRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<Result<Guid>> Handle(SyncProductFromExternalCommand request, CancellationToken ct) {
		Product? product = await _productRepository.GetByExternalIdAsync(request.ExternalId, ct);

		if (product == null) {
			product = Product.Create(request.Name, request.Description, request.ExternalId);

			await _productRepository.AddAsync(product, ct);

			_logger.LogInformation("Product with external id {ExternalId} created successfully.", request.ExternalId);
		} else {
			product.SetName(request.Name);
			product.SetDescription(request.Description);

			_productRepository.Update(product);

			_logger.LogInformation("Product with external id {ExternalId} updated successfully.", request.ExternalId);
		}

		await _unitOfWork.CommitAsync(ct);
		return Result.Success(product.Id);
	}
}
