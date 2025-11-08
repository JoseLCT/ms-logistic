using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.Order.Entities;
using MsLogistic.Infrastructure.Shared.Utils.Parsers;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class OrderDeliveryConfig : IEntityTypeConfiguration<OrderDelivery>
{
    public void Configure(EntityTypeBuilder<OrderDelivery> builder)
    {
        builder.ToTable("order_deliveries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id");

        builder.Property(x => x.DeliveryPersonId)
            .HasColumnName("delivery_person_id");

        builder.Property(x => x.Location)
            .HasColumnName("location")
            .HasConversion(
                v => GeoPointParser.ConvertToPoint(v),
                v => PointParser.ConvertToGeoPointValue(v)
            );

        builder.Property(x => x.DeliveredAt)
            .HasColumnName("delivered_at");

        builder.Property(x => x.Comments)
            .HasColumnName("comments");

        builder.Property(x => x.ImageUrl)
            .HasColumnName("image_url");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
    }
}