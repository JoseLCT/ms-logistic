using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Drivers.Repositories;
using MsLogistic.Domain.Shared.Errors;

namespace MsLogistic.Application.Drivers.RemoveDriver;

public class RemoveDriverHandler : IRequestHandler<RemoveDriverCommand, Result<Guid>> {
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveDriverHandler> _logger;

    public RemoveDriverHandler(
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ILogger<RemoveDriverHandler> logger
    ) {
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RemoveDriverCommand request, CancellationToken ct) {
        var driver = await _driverRepository.GetByIdAsync(request.Id, ct);

        if (driver is null) {
            return Result.Failure<Guid>(
                CommonErrors.NotFoundById("Driver", request.Id)
            );
        }

        _driverRepository.Remove(driver);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Driver with id {DriverId} removed successfully.", driver.Id);

        return Result.Success(driver.Id);
    }
}
