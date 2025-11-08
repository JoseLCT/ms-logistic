using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("order_deliveries")]
internal class OrderDeliveryPersistenceModel
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("order_id")]
    [Required]
    public Guid OrderId { get; set; }

    [ForeignKey("OrderId")]
    public required OrderPersistenceModel Order { get; set; }

    [Column("delivery_person_id")]
    [Required]
    public Guid DeliveryPersonId { get; set; }

    [ForeignKey("DeliveryPersonId")]
    public required DeliveryPersonPersistenceModel DeliveryPerson { get; set; }

    [Column("location", TypeName = "geography")]
    [Required]
    public required Point Location { get; set; }

    [Column("delivered_at")]
    [Required]
    public DateTime DeliveredAt { get; set; }

    [Column("comments")]
    [StringLength(500)]
    public string? Comments { get; set; }

    [Column("image_url")]
    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}