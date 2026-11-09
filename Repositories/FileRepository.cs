using Microsoft.EntityFrameworkCore;
using MyApiApp.Data;
using MyApiApp.Models;

namespace MyApiApp.Repositories;

public class FileRepository : IFileRepository
{
    private readonly AppDbContext _context;

    public FileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FileUpload> AddAsync(FileUpload file)
    {
        await _context.FileUploads.AddAsync(file);
        await _context.SaveChangesAsync();
        return file;
    }

    public async Task<List<FileUpload>> GetAllAsync()
    {
        return await _context.FileUploads
            .AsNoTracking()
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync();
    }

    public async Task<FileUpload?> GetByStoredNameAsync(string storedName)
    {
        return await _context.FileUploads
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.StoredName == storedName);
    }

    public async Task<bool> DeleteAsync(string storedName)
    {
        var entity = await _context.FileUploads
            .FirstOrDefaultAsync(f => f.StoredName == storedName);

        if (entity == null)
        {
            return false;
        }

        _context.FileUploads.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
