using MsLogistic.Domain.Batches.Enums;

namespace MsLogistic.Application.Batches.GetBatchById;

public record BatchDetailDto(
    Guid Id,
    int TotalOrders,
    BatchStatusEnum Status,
    DateTime OpenedAt,
    DateTime? ClosedAt
);