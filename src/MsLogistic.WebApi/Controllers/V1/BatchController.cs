using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Batches.GetAllBatches;
using MsLogistic.Application.Batches.GetBatchById;
using MsLogistic.Core.Results;

namespace MsLogistic.WebApi.Controllers.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/batches-test")]
public class BatchController : ApiControllerBase {
	private readonly IMediator _mediator;

	public BatchController(IMediator mediator) {
		_mediator = mediator;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll() {
		var query = new GetAllBatchesQuery();
		Result<IReadOnlyList<BatchSummaryDto>> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpGet("{id:guid}")]
	public async Task<IActionResult> GetById(Guid id) {
		var query = new GetBatchByIdQuery(id);
		Result<BatchDetailDto> result = await _mediator.Send(query);
		return HandleResult(result);
	}
}
