using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MsLogistic.Domain.Drivers.Enums;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

[Table("drivers")]
internal class DriverPersistenceModel {
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("full_name")]
    [MaxLength(100)]
    public required string FullName { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("status")]
    public DriverStatusEnum Status { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
