using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MsLogistic.Domain.Route.Types;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("routes")]
internal class RoutePersistenceModel
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("delivery_zone_id")]
    [Required]
    public Guid DeliveryZoneId { get; set; }
    
    [ForeignKey("DeliveryZoneId")]
    public required DeliveryZonePersistenceModel DeliveryZone { get; set; }

    [Column("delivery_person_id")]
    public Guid? DeliveryPersonId { get; set; }

    [ForeignKey("DeliveryPersonId")]
    public DeliveryPersonPersistenceModel? DeliveryPerson { get; set; }

    [Column("scheduled_date")]
    [Required]
    public DateTime ScheduledDate { get; set; }

    [Column("origin_location", TypeName = "geography")]
    [Required]
    public required Point OriginLocation { get; set; }

    [Column("status")]
    [Required]
    public RouteStatusType Status { get; set; }

    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public ICollection<OrderPersistenceModel> Orders { get; set; } = new List<OrderPersistenceModel>();
}