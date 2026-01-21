using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Batches.GetAllBatches;
using MsLogistic.Application.Batches.GetBatchById;

namespace MsLogistic.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/batches")]
public class BatchController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public BatchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllBatchesQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetBatchByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}