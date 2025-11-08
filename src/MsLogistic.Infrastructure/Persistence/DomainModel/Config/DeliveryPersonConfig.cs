using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.DeliveryPerson.Entities;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class DeliveryPersonConfig : IEntityTypeConfiguration<DeliveryPerson>
{
    public void Configure(EntityTypeBuilder<DeliveryPerson> builder)
    {
        builder.ToTable("delivery_persons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
    }
}