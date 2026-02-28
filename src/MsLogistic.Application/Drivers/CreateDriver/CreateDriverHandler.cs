using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Drivers.Repositories;

namespace MsLogistic.Application.Drivers.CreateDriver;

public class CreateDriverHandler : IRequestHandler<CreateDriverCommand, Result<Guid>> {
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateDriverHandler> _logger;

    public CreateDriverHandler(
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateDriverHandler> logger
    ) {
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateDriverCommand request, CancellationToken ct) {
        var driver = Driver.Create(request.FullName);

        await _driverRepository.AddAsync(driver, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Driver with id {DriverId} created successfully.", driver.Id);

        return Result.Success(driver.Id);
    }
}
