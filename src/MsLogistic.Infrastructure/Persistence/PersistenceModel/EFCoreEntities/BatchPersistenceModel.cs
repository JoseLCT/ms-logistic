using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MsLogistic.Domain.Batches.Enums;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("batches")]
internal class BatchPersistenceModel
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("total_orders")]
    public int TotalOrders { get; set; }
    
    [Column("status")]
    public BatchStatusEnum Status { get; set; }
    
    [Column("opened_at")]
    public DateTime OpenedAt { get; set; }
    
    [Column("closed_at")]
    public DateTime? ClosedAt { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}