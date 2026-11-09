using MyApiApp.DTOs;
using MyApiApp.DTOs.Common;

namespace MyApiApp.Services;

public interface IUserService
{
    // Lấy danh sách User có phân trang/search/filter/sort
    Task<PagedResult<UserResponseDTO>> GetAllAsync(UserQueryParameters queryParams);

    Task<UserResponseDTO?> GetByIdAsync(int id);
    Task<UserResponseDTO> RegisterAsync(UserCreateDTO request);
    Task<UserResponseDTO?> UpdateAsync(int id, UserUpdateDTO request);
    Task<bool> DeleteAsync(int id);
    Task<string?> LoginAsync(UserLoginDTO request);

    // Sinh dữ liệu mẫu (chỉ dùng cho môi trường dev/demo)
    Task<int> SeedRandomAsync(int count);
}
