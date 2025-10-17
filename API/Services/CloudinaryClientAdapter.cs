using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services;

public class CloudinaryClientAdapter : ICloudinaryClient
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryClientAdapter(IOptions<CloudinarySettings> config)
    {
        ArgumentNullException.ThrowIfNull(config);
        
        var settings = config.Value;
        if (string.IsNullOrWhiteSpace(settings.CloudName) ||
            string.IsNullOrWhiteSpace(settings.ApiKey) ||
            string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            throw new InvalidOperationException(
                "La configuración de Cloudinary no es válida. Por favor, verifica CloudinarySettings en las configuraciones de la aplicación.");
        }
        
        var acc = new Account(
            settings.CloudName,
            settings.ApiKey,
            settings.ApiSecret
        );

        _cloudinary = new Cloudinary(acc);
    }

    public Task<ImageUploadResult> UploadAsync(ImageUploadParams uploadParams) =>
        _cloudinary.UploadAsync(uploadParams);

    public Task<DeletionResult> DestroyAsync(DeletionParams deletionParams) =>
        _cloudinary.DestroyAsync(deletionParams);
}
