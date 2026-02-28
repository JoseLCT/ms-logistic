using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.Drivers.Entities;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class DriverConfig : IEntityTypeConfiguration<Driver> {
    public void Configure(EntityTypeBuilder<Driver> builder) {
        builder.ToTable("drivers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(x => x.DomainEvents);
    }
}
