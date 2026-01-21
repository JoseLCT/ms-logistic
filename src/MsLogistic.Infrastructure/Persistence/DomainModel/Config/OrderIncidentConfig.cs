using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Orders.Entities;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class OrderIncidentConfig : IEntityTypeConfiguration<OrderIncident>
{
    public void Configure(EntityTypeBuilder<OrderIncident> builder)
    {
        builder.ToTable("order_incidents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id");

        builder.Property(x => x.DriverId)
            .HasColumnName("driver_id");

        builder.Property(x => x.IncidentType)
            .HasColumnName("incident_type")
            .HasConversion<int>();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne<Driver>()
            .WithMany()
            .HasForeignKey(oi => oi.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}