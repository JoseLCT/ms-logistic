using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Logistics.ValueObjects;

public sealed record OrderedWaypoint {
    public Guid WaypointId { get; }
    public int Sequence { get; }

    public OrderedWaypoint(Guid waypointId, int sequence) {
        if (sequence <= 0) {
            throw new DomainException(
                Error.Validation(
                    code: "OrderedWaypoint.Sequence",
                    message: "Sequence must be positive"
                )
            );
        }

        WaypointId = waypointId;
        Sequence = sequence;
    }
}
