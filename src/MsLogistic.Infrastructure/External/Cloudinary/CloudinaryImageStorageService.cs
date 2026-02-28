using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MsLogistic.Application.Abstractions.Services;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.ValueObjects;
using Error = MsLogistic.Core.Results.Error;

namespace MsLogistic.Infrastructure.External.Cloudinary;

public class CloudinaryImageStorageService : IImageStorageService {
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    private readonly CloudinaryOptions _options;
    private readonly ILogger<CloudinaryImageStorageService> _logger;

    public CloudinaryImageStorageService(
        CloudinaryDotNet.Cloudinary cloudinary,
        IOptions<CloudinaryOptions> options,
        ILogger<CloudinaryImageStorageService> logger
    ) {
        _cloudinary = cloudinary;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<ImageResourceValue>> UploadAsync(
        Stream file,
        string fileName,
        CancellationToken ct
    ) {
        try {
            var uploadParams = new ImageUploadParams {
                File = new FileDescription(fileName, file),
                Folder = _options.Folder,
                UseFilename = true,
                UniqueFilename = true
            };

            var result = await _cloudinary.UploadAsync(uploadParams, ct);

            if (result.StatusCode != HttpStatusCode.OK) {
                return Result.Failure<ImageResourceValue>(
                    Error.Failure(
                        code: "CloudinaryUploadFailed",
                        message: result.Error?.Message ?? "Image upload failed."
                    )
                );
            }

            return Result.Success(
                new ImageResourceValue {
                    Url = result.SecureUrl.ToString(),
                    ExternalId = result.PublicId
                }
            );
        } catch (Exception ex) {
            _logger.LogError(ex, "Error uploading image to Cloudinary");

            return Result.Failure<ImageResourceValue>(
                Error.Failure(
                    code: "CloudinaryException",
                    message: "An error occurred while uploading the image."
                )
            );
        }
    }
}
