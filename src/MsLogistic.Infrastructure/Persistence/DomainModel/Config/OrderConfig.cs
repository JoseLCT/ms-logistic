using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.Order.Entities;
using MsLogistic.Infrastructure.Shared.Utils.Parsers;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class OrderConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

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
            .HasColumnName("delivery_address");

        builder.Property(x => x.DeliveryLocation)
            .HasColumnName("delivery_location")
            .HasColumnType("geography")
            .HasConversion(
                v => GeoPointParser.ConvertToPoint(v),
                v => PointParser.ConvertToGeoPointValue(v)
            );

        builder.HasMany("_items");

        builder.HasOne(o => o.Delivery)
            .WithOne()
            .HasForeignKey<OrderDelivery>(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.Items);
    }
}