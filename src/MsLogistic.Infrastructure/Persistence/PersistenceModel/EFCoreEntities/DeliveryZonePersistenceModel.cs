using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("delivery_zones")]
internal class DeliveryZonePersistenceModel
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("delivery_person_id")]
    public Guid? DeliveryPersonId { get; set; }

    [ForeignKey("DeliveryPersonId")]
    public DeliveryPersonPersistenceModel? DeliveryPerson { get; set; }

    [Column("code")]
    [StringLength(10)]
    [Required]
    public required string Code { get; set; }

    [Column("name")]
    [StringLength(100)]
    [Required]
    public required string Name { get; set; }

    [Column("boundaries")]
    [Required]
    public required Polygon Boundaries { get; set; }
    
    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}