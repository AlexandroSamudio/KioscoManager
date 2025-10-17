using System.Text;
using API.Interfaces;
using API.Services;
using CloudinaryDotNet.Actions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace API.Tests.Services;

public class PhotoServiceTests
{
    private static IFormFile MakeFormFile(byte[] content, string fileName = "test.jpg")
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    [Fact]
    public async Task AddPhotoAsync_Returns_Error_When_File_Length_Zero()
    {
        var cloudinaryMock = new Mock<ICloudinaryClient>(MockBehavior.Strict);
        var service = new PhotoService(cloudinaryMock.Object);
        var emptyFile = MakeFormFile(Array.Empty<byte>());

        var result = await service.AddPhotoAsync(emptyFile);

        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("vacío");
        cloudinaryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddPhotoAsync_Calls_Cloudinary_With_Expected_Params()
    {
        var cloudinaryMock = new Mock<ICloudinaryClient>(MockBehavior.Strict);
        cloudinaryMock
            .Setup(m => m.UploadAsync(It.IsAny<ImageUploadParams>()))
            .ReturnsAsync(new ImageUploadResult { PublicId = "pid", SecureUrl = new Uri("https://x/y.jpg") });

        var service = new PhotoService(cloudinaryMock.Object);
        var content = Encoding.UTF8.GetBytes("abc");
        var file = MakeFormFile(content, "pic.jpg");

        var result = await service.AddPhotoAsync(file);

        result.PublicId.Should().Be("pid");

        cloudinaryMock.Verify(m => m.UploadAsync(It.Is<ImageUploadParams>(p =>
            p.Folder == "sistema-gestion-inventario" &&
            p.File!.FileName == "pic.jpg" &&
            p.Transformation != null
        )), Times.Once);

        cloudinaryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeletePhotoAsync_Calls_Cloudinary_Destroy_With_PublicId()
    {
        var cloudinaryMock = new Mock<ICloudinaryClient>(MockBehavior.Strict);
        cloudinaryMock
            .Setup(m => m.DestroyAsync(It.IsAny<DeletionParams>()))
            .ReturnsAsync(new DeletionResult { Result = "ok" });

        var service = new PhotoService(cloudinaryMock.Object);
        var result = await service.DeletePhotoAsync("public123");

        result.Result.Should().Be("ok");

        cloudinaryMock.Verify(m => m.DestroyAsync(It.Is<DeletionParams>(d => d.PublicId == "public123")), Times.Once);
        cloudinaryMock.VerifyNoOtherCalls();
    }
}
