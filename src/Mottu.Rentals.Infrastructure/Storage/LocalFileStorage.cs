using Mottu.Rentals.Application.Abstractions;

namespace Mottu.Rentals.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _root;
    public LocalFileStorage()
    {
        _root = Path.Combine(AppContext.BaseDirectory, "storage");
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(string fileName, Stream content, CancellationToken ct)
    {
        var safeName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        var path = Path.Combine(_root, safeName);
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs, ct);
        return path;
    }
}


