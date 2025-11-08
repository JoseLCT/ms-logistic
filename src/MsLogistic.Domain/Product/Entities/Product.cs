using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Product.Errors;

namespace MsLogistic.Domain.Product.Entities;

public class Product : AggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }

    private Product()
    {
    }

    public Product(string name, string? description) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(ProductErrors.NameIsRequired);
        }

        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(ProductErrors.NameIsRequired);
        }

        Name = name;
        MarkAsUpdated();
    }

    public void SetDescription(string? description)
    {
        Description = description;
        MarkAsUpdated();
    }
}