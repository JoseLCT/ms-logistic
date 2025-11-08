using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("customers")]
internal class CustomerPersistenceModel
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    [Required]
    public required string Name { get; set; }

    [Column("phone_number")]
    [StringLength(15)]
    [Required]
    public required string PhoneNumber { get; set; }

    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}