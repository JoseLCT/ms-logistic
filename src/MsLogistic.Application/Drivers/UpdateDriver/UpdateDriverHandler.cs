using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Drivers.Repositories;
using MsLogistic.Domain.Shared.Errors;

namespace MsLogistic.Application.Drivers.UpdateDriver;

public class UpdateDriverHandler : IRequestHandler<UpdateDriverCommand, Result<Guid>> {
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateDriverHandler> _logger;

    public UpdateDriverHandler(
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateDriverHandler> logger
    ) {
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UpdateDriverCommand request, CancellationToken ct) {
        var driver = await _driverRepository.GetByIdAsync(request.Id, ct);

        if (driver is null) {
            return Result.Failure<Guid>(
                CommonErrors.NotFoundById("Driver", request.Id)
            );
        }

        driver.SetFullName(request.FullName);
        driver.SetIsActive(request.IsActive);

        _driverRepository.Update(driver);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Driver with id {DriverId} updated successfully.", driver.Id);

        return Result.Success(driver.Id);
    }
}
