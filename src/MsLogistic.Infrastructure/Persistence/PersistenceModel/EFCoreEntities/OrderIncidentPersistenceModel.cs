using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MsLogistic.Domain.Orders.Enums;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("order_incidents")]
internal class OrderIncidentPersistenceModel {
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("order_id")]
    public Guid OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public required OrderPersistenceModel Order { get; set; }

    [Column("driver_id")]
    public Guid DriverId { get; set; }

    [Column("incident_type")]
    public OrderIncidentTypeEnum IncidentType { get; set; }

    [Column("description")]
    [StringLength(500)]
    public required string Description { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
