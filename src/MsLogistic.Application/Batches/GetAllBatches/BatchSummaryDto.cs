using MsLogistic.Domain.Batches.Enums;

namespace MsLogistic.Application.Batches.GetAllBatches;

public record BatchSummaryDto(
    Guid Id,
    BatchStatusEnum Status,
    DateTime OpenedAt,
    DateTime? ClosedAt
);