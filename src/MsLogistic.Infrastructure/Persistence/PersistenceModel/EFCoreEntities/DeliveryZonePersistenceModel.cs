using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("delivery_zones")]
internal class DeliveryZonePersistenceModel {
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("driver_id")]
    public Guid? DriverId { get; set; }

    [Column("code")]
    [StringLength(7)]
    public required string Code { get; set; }

    [Column("name")]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Column("boundaries")]
    public required Polygon Boundaries { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
