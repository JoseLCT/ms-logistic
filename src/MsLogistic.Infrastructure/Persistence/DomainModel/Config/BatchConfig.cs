using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.Batches.Entities;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class BatchConfig : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.ToTable("batches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.TotalOrders)
            .HasColumnName("total_orders");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>();

        builder.Property(x => x.OpenedAt)
            .HasColumnName("opened_at");

        builder.Property(x => x.ClosedAt)
            .HasColumnName("closed_at");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(c => c.DomainEvents);
    }
}