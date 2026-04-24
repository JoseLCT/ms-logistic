using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Application.Customers.CreateCustomer;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Batches.Entities;
using MsLogistic.Domain.Batches.Enums;
using MsLogistic.Domain.Batches.Repositories;

namespace MsLogistic.Application.Batches.CloseCurrentBatch;

public class CloseCurrentBatchHandler : IRequestHandler<CloseCurrentBatchCommand, Result> {
	private readonly IBatchRepository _batchRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<CreateCustomerHandler> _logger;

	public CloseCurrentBatchHandler(
		IBatchRepository batchRepository,
		IUnitOfWork unitOfWork,
		ILogger<CreateCustomerHandler> logger
	) {
		_batchRepository = batchRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<Result> Handle(CloseCurrentBatchCommand request, CancellationToken ct) {
		Batch? batch = await _batchRepository.GetLatestBatchAsync(ct);

		if (batch == null) {
			_logger.LogWarning("Attempted to close batch, but no open batch is available.");
			return Result.Failure(CloseCurrentBatchErrors.NoOpenBatchAvailable);
		}

		if (batch.Status == BatchStatusEnum.Closed) {
			_logger.LogWarning("Attempted to close batch with id {BatchId}, but it is already closed.", batch.Id);
			return Result.Failure(CloseCurrentBatchErrors.LatestBatchAlreadyClosed);
		}

		batch.Close();
		_batchRepository.Update(batch);
		await _unitOfWork.CommitAsync(ct);

		_logger.LogInformation("Batch with id {BatchId} closed successfully.", batch.Id);

		return Result.Success();
	}
}
