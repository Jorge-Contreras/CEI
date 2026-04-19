using System.Security.Cryptography;
using CEI.Application.Common.Interfaces;
using CEI.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CEI.Infrastructure.Services;

public sealed class FileSystemStorageService(IOptions<FileStorageOptions> options) : IFileStorageService
{
    private readonly string _rootPath = EnsureRoot(options.Value.RootPath);

    public async Task<FileStorageWriteResult> SaveAsync(FileStorageWriteRequest request, Stream content, CancellationToken cancellationToken = default)
    {
        var folderName = SanitizePathSegment(request.FolderName);
        var extension = Path.GetExtension(request.FileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var folderPath = Path.Combine(_rootPath, folderName);
        Directory.CreateDirectory(folderPath);

        var fullPath = Path.Combine(folderPath, storedFileName);
        await using var output = File.Create(fullPath);
        using var sha = SHA256.Create();
        await using var crypto = new CryptoStream(output, sha, CryptoStreamMode.Write);
        await content.CopyToAsync(crypto, cancellationToken);
        await crypto.FlushAsync(cancellationToken);
        await output.FlushAsync(cancellationToken);

        var fileInfo = new FileInfo(fullPath);
        var hash = Convert.ToHexString(sha.Hash ?? []);

        return new FileStorageWriteResult(
            Path.GetRelativePath(_rootPath, fullPath),
            storedFileName,
            fileInfo.Length,
            hash);
    }

    public Task<FileStorageReadResult> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storageKey);
        var stream = File.OpenRead(fullPath);
        var contentType = Path.GetExtension(fullPath).Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            ? "application/pdf"
            : "application/octet-stream";

        return Task.FromResult(new FileStorageReadResult(stream, contentType, Path.GetFileName(fullPath)));
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storageKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private static string EnsureRoot(string configuredPath)
    {
        var root = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CEI", "Storage", "Development")
            : configuredPath;

        Directory.CreateDirectory(root);
        return root;
    }

    private static string SanitizePathSegment(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
    }
}
