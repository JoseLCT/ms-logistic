using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.Batches.Entities;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Routes.Entities;
using MsLogistic.Infrastructure.Shared.Utils.Parsers;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class OrderConfig : IEntityTypeConfiguration<Order> {
    public void Configure(EntityTypeBuilder<Order> builder) {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.BatchId)
            .HasColumnName("batch_id");

        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id");

        builder.Property(x => x.RouteId)
            .HasColumnName("route_id");

        builder.Property(x => x.DeliverySequence)
            .HasColumnName("delivery_sequence");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>();

        builder.Property(x => x.ScheduledDeliveryDate)
            .HasColumnName("scheduled_delivery_date");

        builder.Property(x => x.DeliveryAddress)
            .HasColumnName("delivery_address")
            .HasMaxLength(500);

        builder.Property(x => x.DeliveryLocation)
            .HasColumnName("delivery_location")
            .HasColumnType("geography")
            .HasConversion(
                geoPoint => GeoPointParser.ConvertToPoint(geoPoint),
                point => PointParser.ConvertToGeoPointValue(point)
            );

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(o => o.Delivery)
            .WithOne()
            .HasForeignKey<OrderDelivery>(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Incident)
            .WithOne()
            .HasForeignKey<OrderIncident>(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Batch>()
            .WithMany()
            .HasForeignKey(o => o.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Route>()
            .WithMany()
            .HasForeignKey(o => o.RouteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Metadata
            .FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }
}
