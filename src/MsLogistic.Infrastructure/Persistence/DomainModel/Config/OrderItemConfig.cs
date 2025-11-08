using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.Order.Entities;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id");

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id");

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
    }
}