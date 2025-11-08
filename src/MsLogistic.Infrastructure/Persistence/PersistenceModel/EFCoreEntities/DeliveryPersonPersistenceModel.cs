using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MsLogistic.Domain.DeliveryPerson.Types;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("delivery_persons")]
internal class DeliveryPersonPersistenceModel
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    [Required]
    public required string Name { get; set; }

    [Column("is_active")]
    [Required]
    public bool IsActive { get; set; }

    [Column("status")]
    [Required]
    public DeliveryPersonStatusType Status { get; set; }

    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}