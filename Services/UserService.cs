using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MyApiApp.DTOs;
using MyApiApp.DTOs.Common;
using MyApiApp.Models;
using MyApiApp.Repositories;

namespace MyApiApp.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _configuration;

    // Inject Repository (truy cập DB qua EF Core) và IConfiguration (đọc JWT Secret)
    public UserService(IUserRepository userRepo, IConfiguration configuration)
    {
        _userRepo = userRepo;
        _configuration = configuration;
    }

    // 1. Lấy danh sách người dùng (có phân trang/search/filter/sort)
    public async Task<PagedResult<UserResponseDTO>> GetAllAsync(UserQueryParameters queryParams)
    {
        var pagedUsers = await _userRepo.GetAllAsync(queryParams);

        // Map danh sách Entity sang DTO
        var mappedItems = pagedUsers.Items.Select(ToResponse).ToList();

        // Trả về PagedResult chứa DTO
        return new PagedResult<UserResponseDTO>(
            mappedItems,
            pagedUsers.TotalCount,
            pagedUsers.PageNumber,
            pagedUsers.PageSize
        );
    }

    // 2. Lấy người dùng theo Id
    public async Task<UserResponseDTO?> GetByIdAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        return user == null ? null : ToResponse(user);
    }

    // 3. Đăng ký tài khoản mới
    public async Task<UserResponseDTO> RegisterAsync(UserCreateDTO request)
    {
        if (await _userRepo.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException($"Email '{request.Email}' đã được đăng ký.");
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);
        return ToResponse(user);
    }

    // 4. Cập nhật thông tin người dùng
    public async Task<UserResponseDTO?> UpdateAsync(int id, UserUpdateDTO request)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.FullName)) user.FullName = request.FullName;
        if (!string.IsNullOrWhiteSpace(request.Email)) user.Email = request.Email;
        if (!string.IsNullOrWhiteSpace(request.Password)) user.PasswordHash = HashPassword(request.Password);
        if (!string.IsNullOrWhiteSpace(request.Role)) user.Role = request.Role;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);
        return ToResponse(user);
    }

    // 5. Xóa người dùng
    public Task<bool> DeleteAsync(int id) => _userRepo.DeleteAsync(id);

    // 6. Đăng nhập -> trả về JWT Token (trả null nếu sai email/mật khẩu)
    public async Task<string?> LoginAsync(UserLoginDTO request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        return GenerateJwtToken(user);
    }

    // 7. Sinh ngẫu nhiên N user mẫu (dev/demo)
    public async Task<int> SeedRandomAsync(int count)
    {
        if (count <= 0) return 0;
        if (count > 2000) count = 2000; // chặn lạm dụng

        var ho = new[] { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý", "Trịnh", "Đinh", "Tạ", "Phan", "Mai", "Cao" };
        var dem = new[] { "Văn", "Thị", "Minh", "Quốc", "Thanh", "Hoàng", "Ngọc", "Hữu", "Đức", "Thu", "Anh", "Bảo", "Khánh", "Gia", "Tuấn", "Phương", "Hải", "Trung", "Diệu", "Kim" };
        var ten = new[] { "An", "Bình", "Cường", "Dũng", "Em", "Giang", "Huy", "Hạnh", "Khánh", "Lan", "Linh", "Minh", "Nam", "Nga", "Oanh", "Phúc", "Quỳnh", "Sơn", "Trang", "Tú", "Uyên", "Vinh", "Yến", "Thảo", "Hùng", "Kiên", "Lộc", "Mai", "Nhung", "Phong" };
        var roles = new[] { "Admin", "User", "User", "User", "User", "Editor" };

        var rnd = new Random();
        var existingEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Lấy email đã có để tránh vi phạm unique index
        var existing = await _userRepo.GetAllAsync(new UserQueryParameters { PageNumber = 1, PageSize = 50 });
        foreach (var u in existing.Items) existingEmails.Add(u.Email);

        var users = new List<User>(count);
        var attempts = 0;

        while (users.Count < count && attempts < count * 20)
        {
            attempts++;

            var fullName = $"{ho[rnd.Next(ho.Length)]} {dem[rnd.Next(dem.Length)]} {ten[rnd.Next(ten.Length)]}";
            var email = $"user{DateTime.UtcNow.Ticks % 100000000}{users.Count}{rnd.Next(1000, 9999)}@example.com";

            if (!existingEmails.Add(email)) continue;

            var createdAt = DateTime.UtcNow.AddDays(-rnd.Next(0, 720)).AddMinutes(-rnd.Next(0, 1440));

            users.Add(new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = HashPassword("123456"),
                Role = roles[rnd.Next(roles.Length)],
                IsActive = rnd.Next(100) < 85, // ~85% đang hoạt động
                CreatedAt = createdAt,
                UpdatedAt = rnd.Next(100) < 30 ? createdAt.AddDays(rnd.Next(1, 30)) : null
            });
        }

        return await _userRepo.AddRangeAsync(users);
    }

    // ---------- Helper ----------

    // Ánh xạ Entity User -> DTO UserResponseDTO (tránh lộ PasswordHash ra ngoài)
    private static UserResponseDTO ToResponse(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };

    // Băm mật khẩu bằng SHA256 (bản demo - production nên dùng BCrypt/Argon2)
    private static string HashPassword(string password)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

    private static bool VerifyPassword(string password, string passwordHash)
        => HashPassword(password).Equals(passwordHash, StringComparison.OrdinalIgnoreCase);

    // Tạo JWT Token thủ công bằng HMAC-SHA256 (không cần NuGet package).
    // Trong thực tế nên dùng thư viện System.IdentityModel.Tokens.Jwt + JwtBearer.
    private string GenerateJwtToken(User user)
    {
        var secret = _configuration["Jwt:Secret"] ?? "MyApiApp_Default_Secret_Key_Do_Not_Use_In_Production!";

        var header = Base64UrlEncode(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));

        var payload = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
        {
            sub = user.Id.ToString(),
            email = user.Email,
            role = user.Role,
            exp = DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds()
        })));

        var signatureInput = $"{header}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signature = Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureInput)));

        return $"{signatureInput}.{signature}";
    }

    private static string Base64UrlEncode(byte[] data)
        => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
