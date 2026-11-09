using MyApiApp.DTOs;
using MyApiApp.Models;

namespace MyApiApp.Services;

public interface IFileService
{
    Task<FileResponseDTO> UploadAsync(FileUploadDTO request);

    Task<List<FileResponseDTO>> GetAllAsync();

    Task<FileUpload?> GetByStoredNameAsync(string storedName);

    string? GetPhysicalPath(string storedName);

    Task<bool> DeleteAsync(string storedName);
}
