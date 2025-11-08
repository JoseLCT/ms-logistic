using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MsLogistic.Domain.Order.Types;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("orders")]
internal class OrderPersistenceModel
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("customer_id")]
    [Required]
    public Guid CustomerId { get; set; }
    
    [ForeignKey("CustomerId")]
    public required CustomerPersistenceModel Customer { get; set; }
    
    [Column("route_id")]
    public Guid? RouteId { get; set; }
    
    [ForeignKey("RouteId")]
    public RoutePersistenceModel? Route { get; set; }
    
    [Column("delivery_sequence")]
    public int? DeliverySequence { get; set; }
    
    [Column("status")]
    [Required]
    public OrderStatusType Status { get; set; }
    
    [Column("scheduled_delivery_date")]
    [Required]
    public DateTime ScheduledDeliveryDate { get; set; }
    
    [Column("delivery_address")]
    [StringLength(500)]
    [Required]
    public required string DeliveryAddress { get; set; }
    
    [Column("delivery_location", TypeName = "geography")]
    [Required]
    public required Point DeliveryLocation { get; set; }
    
    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    public ICollection<OrderItemPersistenceModel> Items { get; set; } = new List<OrderItemPersistenceModel>();
    public OrderDeliveryPersistenceModel? Delivery { get; set; }
}