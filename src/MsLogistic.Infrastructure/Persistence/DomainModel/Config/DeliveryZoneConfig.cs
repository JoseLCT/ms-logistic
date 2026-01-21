using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.DeliveryZones.Entities;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Infrastructure.Shared.Utils.Parsers;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class DeliveryZoneConfig : IEntityTypeConfiguration<DeliveryZone>
{
    public void Configure(EntityTypeBuilder<DeliveryZone> builder)
    {
        builder.ToTable("delivery_zones");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.DriverId)
            .HasColumnName("driver_id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(7);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100);

        builder.Property(x => x.Boundaries)
            .HasColumnName("boundaries")
            .HasColumnType("geography")
            .HasConversion(
                boundaries => BoundariesParser.ConvertToPolygon(boundaries),
                polygon => PolygonParser.ConvertToBoundariesValue(polygon)
            );

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne<Driver>()
            .WithMany()
            .HasForeignKey(dz => dz.DriverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(x => x.DomainEvents);
    }
}