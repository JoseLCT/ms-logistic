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
	[ProducesResponseType(typeof(Result<IReadOnlyList<ProductSummaryDto>>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetAll() {
		var query = new GetAllProductsQuery();
		Result<IReadOnlyList<ProductSummaryDto>> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(Result<ProductDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(Result<ProductDetailDto>), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(Guid id) {
		var query = new GetProductByIdQuery(id);
		Result<ProductDetailDto> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpPost]
	[ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
	public async Task<IActionResult> Create([FromBody] CreateProductContract contract) {
		var command = new CreateProductCommand(contract.Name, contract.Description);
		Result<Guid> result = await _mediator.Send(command);
		return HandleCreatedResult(
			result,
			nameof(GetById),
			new { id = result.IsSuccess ? result.Value : Guid.Empty }
		);
	}

	[HttpPut("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductContract contract) {
		var command = new UpdateProductCommand(id, contract.Name, contract.Description);
		Result result = await _mediator.Send(command);
		return HandleNoContentResult(result);
	}

	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Remove(Guid id) {
		var command = new RemoveProductCommand(id);
		Result result = await _mediator.Send(command);
		return HandleNoContentResult(result);
	}
}
