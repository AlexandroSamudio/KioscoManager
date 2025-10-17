using CloudinaryDotNet.Actions;

namespace API.Interfaces;

public interface ICloudinaryClient
{
    Task<ImageUploadResult> UploadAsync(ImageUploadParams uploadParams);
    Task<DeletionResult> DestroyAsync(DeletionParams deletionParams);
}
