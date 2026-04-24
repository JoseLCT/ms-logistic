using MsLogistic.Core.Results;

namespace MsLogistic.Application.Batches.CloseCurrentBatch;

public static class CloseCurrentBatchErrors {
	public static Error NoOpenBatchAvailable =>
		Error.NotFound(
			code: "Batch.Close.NoOpenBatchAvailable",
			message: "There is no open batch available to close."
		);

	public static Error LatestBatchAlreadyClosed =>
		Error.Conflict(
			code: "Batch.Close.LatestBatchAlreadyClosed",
			message: "The latest batch is already closed."
		);
}
