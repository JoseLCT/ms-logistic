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
    public Guid OrderId { get; set; }
    
    [ForeignKey(nameof(OrderId))]
    public required OrderPersistenceModel Order { get; set; }
    
    [Column("driver_id")]
    public Guid DriverId { get; set; }
    
    [Column("location", TypeName = "geography")]
    public required Point Location { get; set; }
    
    [Column("delivered_at")]
    public DateTime DeliveredAt { get; set; }
    
    [Column("comments")]
    [StringLength(500)]
    public string? Comments { get; set; }
    
    [Column("image_url")]
    [StringLength(250)]
    public string? ImageUrl { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}