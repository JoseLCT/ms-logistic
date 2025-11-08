using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.DeliveryZone.Entities;
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

        builder.Property(x => x.DeliveryPersonId)
            .HasColumnName("delivery_person_id");

        builder.Property(x => x.Code)
            .HasColumnName("code");

        builder.Property(x => x.Name)
            .HasColumnName("name");

        builder.Property(x => x.Boundaries)
            .HasColumnName("boundaries")
            .HasColumnType("geography")
            .HasConversion(
                v => ZoneBoundaryParser.ConvertToPolygon(v),
                v => PolygonParser.ConvertToZoneBoundaryValue(v)
            );

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
    }
}