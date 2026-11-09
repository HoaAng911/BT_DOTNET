using MyApiApp.DTOs;
using MyApiApp.Models;
using MyApiApp.Repositories;
using MyApiApp.Validation;

namespace MyApiApp.Services;

public class FileService : IFileService
{
    private const string UploadFolderName = "uploads";

    private readonly IWebHostEnvironment _env;
    private readonly IFileRepository _fileRepository;

    public FileService(IWebHostEnvironment env, IFileRepository fileRepository)
    {
        _env = env;
        _fileRepository = fileRepository;
    }

    private string UploadDirectory =>
        Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), UploadFolderName);

    public async Task<FileResponseDTO> UploadAsync(FileUploadDTO request)
    {
        var file = request.File;

        var errors = FileValidator.Validate(file);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" ", errors));
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        Directory.CreateDirectory(UploadDirectory);

        var storedName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(UploadDirectory, storedName);

        await using (var stream = new FileStream(physicalPath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream);
        }

        var entity = new FileUpload
        {
            OriginalName = Path.GetFileName(file.FileName),
            StoredName = storedName,
            Url = $"/{UploadFolderName}/{storedName}",
            Size = file.Length,
            ContentType = file.ContentType,
            Extension = extension,
            Description = request.Description,
            UploadedAt = DateTime.UtcNow
        };

        try
        {
            entity = await _fileRepository.AddAsync(entity);
        }
        catch
        {
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
            throw;
        }

        return ToResponse(entity);
    }

    public async Task<List<FileResponseDTO>> GetAllAsync()
    {
        var entities = await _fileRepository.GetAllAsync();
        return entities.Select(ToResponse).ToList();
    }

    public async Task<FileUpload?> GetByStoredNameAsync(string storedName)
    {
        var safeName = NormalizeStoredName(storedName);
        if (safeName == null)
        {
            return null;
        }

        return await _fileRepository.GetByStoredNameAsync(safeName);
    }

    public string? GetPhysicalPath(string storedName)
    {
        var safeName = NormalizeStoredName(storedName);
        if (safeName == null)
        {
            return null;
        }

        var path = Path.Combine(UploadDirectory, safeName);
        return File.Exists(path) ? path : null;
    }

    private static string? NormalizeStoredName(string storedName)
    {
        if (string.IsNullOrWhiteSpace(storedName))
        {
            return null;
        }

        var safeName = Path.GetFileName(storedName);
        return string.IsNullOrWhiteSpace(safeName) || safeName != storedName ? null : safeName;
    }

    public async Task<bool> DeleteAsync(string storedName)
    {
        var safeName = NormalizeStoredName(storedName);
        if (safeName == null)
        {
            return false;
        }

        var path = GetPhysicalPath(safeName);

        var deletedInDb = await _fileRepository.DeleteAsync(safeName);

        if (path != null)
        {
            File.Delete(path);
        }

        return deletedInDb || path != null;
    }

    private static FileResponseDTO ToResponse(FileUpload entity) => new()
    {
        OriginalName = entity.OriginalName,
        StoredName = entity.StoredName,
        Url = entity.Url,
        Size = entity.Size,
        SizeDisplay = FormatSize(entity.Size),
        ContentType = entity.ContentType,
        Description = entity.Description,
        UploadedAt = entity.UploadedAt
    };

    private static string FormatSize(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB" };
        double size = bytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }
}
