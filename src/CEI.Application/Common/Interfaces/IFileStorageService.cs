namespace CEI.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<FileStorageWriteResult> SaveAsync(FileStorageWriteRequest request, Stream content, CancellationToken cancellationToken = default);

    Task<FileStorageReadResult> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}

public sealed record FileStorageWriteRequest(
    string FileName,
    string ContentType,
    string FolderName);

public sealed record FileStorageWriteResult(
    string StorageKey,
    string StoredFileName,
    long FileSizeBytes,
    string Sha256Hash);

public sealed record FileStorageReadResult(
    Stream Content,
    string ContentType,
    string FileName);
