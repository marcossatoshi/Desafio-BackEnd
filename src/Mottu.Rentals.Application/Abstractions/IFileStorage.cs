namespace Mottu.Rentals.Application.Abstractions;

public interface IFileStorage
{
    Task<string> SaveAsync(string fileName, Stream content, CancellationToken ct);
}


