using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MsLogistic.Domain.Orders.Enums;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("orders")]
internal class OrderPersistenceModel {
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("batch_id")]
    public Guid BatchId { get; set; }

    [Column("customer_id")]
    public Guid CustomerId { get; set; }

    [Column("route_id")]
    public Guid? RouteId { get; set; }

    [Column("delivery_sequence")]
    public int? DeliverySequence { get; set; }

    [Column("status")]
    public OrderStatusEnum Status { get; set; }

    [Column("scheduled_delivery_date")]
    public DateTime ScheduledDeliveryDate { get; set; }

    [Column("delivery_address")]
    [StringLength(500)]
    public required string DeliveryAddress { get; set; }

    [Column("delivery_location", TypeName = "geography")]
    public required Point DeliveryLocation { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public ICollection<OrderItemPersistenceModel> Items { get; set; } = [];
    public OrderDeliveryPersistenceModel? Delivery { get; set; }
    public OrderIncidentPersistenceModel? Incident { get; set; }
}
