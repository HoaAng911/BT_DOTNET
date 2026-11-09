using Microsoft.EntityFrameworkCore;
using MyApiApp.Data;
using MyApiApp.DTOs.Common;
using MyApiApp.Models;

namespace MyApiApp.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<User>> GetAllAsync(UserQueryParameters queryParams)
    {
        var query = _db.Users.AsNoTracking().AsQueryable();

        // 1. SEARCH (Tìm kiếm tương đối)
        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var term = queryParams.SearchTerm.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(term)
                                  || u.Email.ToLower().Contains(term));
        }

        // 2. FILTER (Lọc chính xác)
        if (!string.IsNullOrWhiteSpace(queryParams.Role))
        {
            query = query.Where(u => u.Role == queryParams.Role);
        }

        //if (queryParams.IsActive.HasValue)
        //{
        //    query = query.Where(u => u.IsActive == queryParams.IsActive.Value);
        //}

        // 3. SORTING
        query = queryParams.SortBy?.ToLower() switch
        {
            "email" => queryParams.IsDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),

            "createdat" => queryParams.IsDescending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),

            _ => queryParams.IsDescending
                ? query.OrderByDescending(u => u.FullName)
                : query.OrderBy(u => u.FullName) // Mặc định sort theo FullName
        };

        // 4. COUNT (Tổng số dòng thỏa điều kiện search/filter)
        var totalCount = await query.CountAsync();

        // 5. PAGINATION (Chỉ lấy đúng PageSize dòng từ DB)
        var pageNumber = queryParams.PageNumber < 1 ? 1 : queryParams.PageNumber;
        var pageSize = queryParams.PageSize < 1 ? 10 : queryParams.PageSize;

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<User>(items, totalCount, pageNumber, pageSize);
    }

    public Task<User?> GetByIdAsync(int id)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(string email)
        => _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<User> AddAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<int> AddRangeAsync(IEnumerable<User> users)
    {
        var list = users.ToList();
        if (list.Count == 0)
        {
            return 0;
        }

        _db.Users.AddRange(list);
        await _db.SaveChangesAsync();
        return list.Count;
    }

    public async Task<User?> UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return false;
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        var normalized = email.ToLower();
        return _db.Users.AnyAsync(u =>
            u.Email.ToLower() == normalized &&
            (!excludeId.HasValue || u.Id != excludeId.Value));
    }
}
