using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MsLogistic.Domain.Routes.Enums;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("routes")]
internal class RoutePersistenceModel {
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("batch_id")]
    public Guid BatchId { get; set; }

    [Column("delivery_zone_id")]
    public Guid DeliveryZoneId { get; set; }

    [Column("driver_id")]
    public Guid? DriverId { get; set; }

    [Column("origin_location", TypeName = "geography")]
    public required Point OriginLocation { get; set; }

    [Column("status")]
    public RouteStatusEnum Status { get; set; }

    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
