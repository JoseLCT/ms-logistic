using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("order_items")]
internal class OrderItemPersistenceModel
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("order_id")]
    [Required]
    public Guid OrderId { get; set; }
    
    [ForeignKey("OrderId")]
    public required OrderPersistenceModel Order { get; set; }
    
    [Column("product_id")]
    [Required]
    public Guid ProductId { get; set; }
    
    [ForeignKey("ProductId")]
    public required ProductPersistenceModel Product { get; set; }
    
    [Column("quantity")]
    [Required]
    public int Quantity { get; set; }
    
    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}