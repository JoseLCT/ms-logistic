using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Products.CreateProduct;
using MsLogistic.Application.Products.GetAllProducts;
using MsLogistic.Application.Products.GetProductById;
using MsLogistic.Application.Products.RemoveProduct;
using MsLogistic.Application.Products.UpdateProduct;
using MsLogistic.Core.Results;
using MsLogistic.WebApi.Contracts.V1.Products;

namespace MsLogistic.WebApi.Controllers.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/products")]
public class ProductController : ApiControllerBase {
	private readonly IMediator _mediator;

	public ProductController(IMediator mediator) {
		_mediator = mediator;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll() {
		var query = new GetAllProductsQuery();
		Result<IReadOnlyList<ProductSummaryDto>> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpGet("{id:guid}")]
	public async Task<IActionResult> GetById(Guid id) {
		var query = new GetProductByIdQuery(id);
		Result<ProductDetailDto> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateProductContract contract) {
		var command = new CreateProductCommand(contract.Name, contract.Description);
		Result<Guid> result = await _mediator.Send(command);
		return HandleResult(result);
	}

	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductContract contract) {
		var command = new UpdateProductCommand(id, contract.Name, contract.Description);
		Result<Guid> result = await _mediator.Send(command);
		return HandleResult(result);
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Remove(Guid id) {
		var command = new RemoveProductCommand(id);
		Result<Guid> result = await _mediator.Send(command);
		return HandleResult(result);
	}
}
