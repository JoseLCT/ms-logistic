using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.Batches.Entities;
using MsLogistic.Domain.DeliveryZones.Entities;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Routes.Entities;
using MsLogistic.Infrastructure.Shared.Utils.Parsers;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class RouteConfig : IEntityTypeConfiguration<Route> {
    public void Configure(EntityTypeBuilder<Route> builder) {
        builder.ToTable("routes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.BatchId)
            .HasColumnName("batch_id");

        builder.Property(x => x.DeliveryZoneId)
            .HasColumnName("delivery_zone_id");

        builder.Property(x => x.DriverId)
            .HasColumnName("driver_id");

        builder.Property(x => x.OriginLocation)
            .HasColumnName("origin_location")
            .HasColumnType("geography")
            .HasConversion(
                geoPoint => GeoPointParser.ConvertToPoint(geoPoint),
                point => PointParser.ConvertToGeoPointValue(point)
            );

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>();

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at");

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne<Batch>()
            .WithMany()
            .HasForeignKey(r => r.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DeliveryZone>()
            .WithMany()
            .HasForeignKey(r => r.DeliveryZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Driver>()
            .WithMany()
            .HasForeignKey(r => r.DriverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(x => x.DomainEvents);
    }
}
