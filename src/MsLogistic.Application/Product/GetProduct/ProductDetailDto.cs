namespace MsLogistic.Application.Product.GetProduct;

public record ProductDetailDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}