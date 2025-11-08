using MediatR;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Product.CreateProduct;
using MsLogistic.Application.Product.DeleteProduct;
using MsLogistic.Application.Product.GetProduct;
using MsLogistic.Application.Product.GetProducts;
using MsLogistic.Application.Product.UpdateProduct;

namespace MsLogistic.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : BaseController
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var query = new GetProductsQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var query = new GetProductQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand request)
    {
        var result = await _mediator.Send(request);
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductCommand request)
    {
        var result = await _mediator.Send(request);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}