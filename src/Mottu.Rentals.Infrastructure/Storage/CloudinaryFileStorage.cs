using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Mottu.Rentals.Application.Abstractions;

namespace Mottu.Rentals.Infrastructure.Storage;

public sealed class CloudinaryFileStorage : IFileStorage
{
    private readonly Cloudinary _cloudinary;
    private readonly string _folder;

    public CloudinaryFileStorage(Cloudinary cloudinary, string folder = "cnh")
    {
        _cloudinary = cloudinary;
        _folder = folder;
    }

    public async Task<string> SaveAsync(string fileName, Stream content, CancellationToken ct)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, content),
            Folder = _folder,
            UseFilename = true,
            UniqueFilename = false,
            Overwrite = true,
            PublicId = Path.GetFileNameWithoutExtension(fileName)
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);
        if (result.StatusCode is System.Net.HttpStatusCode.OK && result.SecureUrl is not null)
        {
            return result.SecureUrl.AbsoluteUri;
        }
        throw new InvalidOperationException($"Cloudinary upload failed: {result.Error?.Message ?? result.StatusCode.ToString()}");
    }
}


