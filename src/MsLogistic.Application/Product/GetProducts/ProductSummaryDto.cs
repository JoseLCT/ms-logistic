namespace MsLogistic.Application.Product.GetProducts;

public record ProductSummaryDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}