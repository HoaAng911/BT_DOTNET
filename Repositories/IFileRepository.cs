using MyApiApp.Models;

namespace MyApiApp.Repositories;

public interface IFileRepository
{
    Task<FileUpload> AddAsync(FileUpload file);

    Task<List<FileUpload>> GetAllAsync();

    Task<FileUpload?> GetByStoredNameAsync(string storedName);

    Task<bool> DeleteAsync(string storedName);
}
