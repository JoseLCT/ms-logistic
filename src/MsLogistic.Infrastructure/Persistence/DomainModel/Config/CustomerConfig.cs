using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Infrastructure.Persistence.DomainModel.Config;

internal class CustomerConfig : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(c => c.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(100);

        builder.Property(c => c.PhoneNumber)
            .HasColumnName("phone_number")
            .HasConversion(
                phoneNumber => phoneNumber != null ? phoneNumber.Value : null,
                value => value != null ? PhoneNumberValue.Create(value) : null
            )
            .HasMaxLength(15);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(c => c.DomainEvents);
    }
}