using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Infrastructure.Shared.Utils.Parsers;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class OrderDeliveryConfig : IEntityTypeConfiguration<OrderDelivery> {
    public void Configure(EntityTypeBuilder<OrderDelivery> builder) {
        builder.ToTable("order_deliveries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id");

        builder.Property(x => x.DriverId)
            .HasColumnName("driver_id");

        builder.Property(x => x.Location)
            .HasColumnName("location")
            .HasColumnType("geography")
            .HasConversion(
                geoPoint => GeoPointParser.ConvertToPoint(geoPoint),
                point => PointParser.ConvertToGeoPointValue(point)
            );

        builder.Property(x => x.DeliveredAt)
            .HasColumnName("delivered_at");

        builder.Property(x => x.Comments)
            .HasColumnName("comments")
            .HasMaxLength(500);

        builder.Property(x => x.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(250);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne<Driver>()
            .WithMany()
            .HasForeignKey(od => od.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}
