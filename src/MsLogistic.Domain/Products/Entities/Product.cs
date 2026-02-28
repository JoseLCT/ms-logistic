using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Products.Errors;

namespace MsLogistic.Domain.Products.Entities;

public class Product : AggregateRoot {
    public string Name { get; private set; }
    public string? Description { get; private set; }

    private Product() {
    }

    private Product(string name, string? description)
        : base(Guid.NewGuid()) {
        Name = name;
        Description = description;
    }

    public static Product Create(string name, string? description) {
        ValidateName(name);
        return new Product(name, description);
    }

    public void SetName(string name) {
        ValidateName(name);
        Name = name;
        MarkAsUpdated();
    }

    public void SetDescription(string? description) {
        Description = description;
        MarkAsUpdated();
    }

    private static void ValidateName(string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            throw new DomainException(ProductErrors.NameIsRequired);
        }
    }
}
