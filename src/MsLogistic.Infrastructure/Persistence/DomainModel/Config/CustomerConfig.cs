using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.Customer.Entities;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class CustomerConfig : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.Name)
            .HasColumnName("name");

        builder.Property(c => c.PhoneNumber)
            .HasColumnName("phone_number")
            .HasConversion(
                v => v.Number,
                v => new PhoneNumberValue(v)
            );

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore("_domainEvents");
        builder.Ignore(c => c.DomainEvents);
    }
}