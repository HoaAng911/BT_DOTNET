using MyApiApp.DTOs.Common;
using MyApiApp.Models;

namespace MyApiApp.Repositories;

public interface IUserRepository
{
    // Lấy danh sách User có phân trang + search + filter + sort
    Task<PagedResult<User>> GetAllAsync(UserQueryParameters queryParams);

    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByEmailAsync(string email);

    Task<User> AddAsync(User user);

    // Thêm nhiều User trong 1 lần SaveChanges (dùng cho seed dữ liệu)
    Task<int> AddRangeAsync(IEnumerable<User> users);

    Task<User?> UpdateAsync(User user);

    Task<bool> DeleteAsync(int id);

    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
