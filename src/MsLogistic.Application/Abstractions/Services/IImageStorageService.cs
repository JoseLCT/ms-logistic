using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.Abstractions.Services;

public interface IImageStorageService {
    Task<Result<ImageResourceValue>> UploadAsync(
        Stream file,
        string fileName,
        CancellationToken ct
    );
}
