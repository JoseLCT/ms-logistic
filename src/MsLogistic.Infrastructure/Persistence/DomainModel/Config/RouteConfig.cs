using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.Route.Entities;
using MsLogistic.Infrastructure.Shared.Utils.Parsers;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class RouteConfig : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("routes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.DeliveryZoneId)
            .HasColumnName("delivery_zone_id");

        builder.Property(x => x.DeliveryPersonId)
            .HasColumnName("delivery_person_id");

        builder.Property(x => x.ScheduledDate)
            .HasColumnName("scheduled_date");

        builder.Property(x => x.OriginLocation)
            .HasColumnName("origin_location")
            .HasConversion(
                v => GeoPointParser.ConvertToPoint(v),
                v => PointParser.ConvertToGeoPointValue(v)
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

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
    }
}