namespace MsLogistic.Application.Products.GetProductById;

public record ProductDetailDto(
    Guid Id,
    string Name,
    string? Description
);
