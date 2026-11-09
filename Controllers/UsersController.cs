using Microsoft.AspNetCore.Mvc;
using MyApiApp.DTOs;
using MyApiApp.DTOs.Common;
using MyApiApp.Services;

namespace MyApiApp.Controllers
{
    // [ApiController]: Bật tính năng Model Binding tự động và validation dữ liệu đầu vào
    [ApiController]

    // ROUTING: Định tuyến URL cấp Controller -> "api/users"
    [Route("api/[controller]")]

    // CONTROLLER: Lớp kế thừa từ ControllerBase để xử lý các request
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        // Inject IUserService vào Controller (Dependency Injection)
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // 1. GET: api/users - Lấy danh sách User (phân trang + search + filter + sort)
        // QUERY PARAMETERS: ?pageNumber=1&pageSize=10&searchTerm=...&role=...&sortBy=...&isDescending=false
        [HttpGet]
        public async Task<ActionResult<PagedResult<UserResponseDTO>>> GetAll([FromQuery] UserQueryParameters queryParams)
        {
            var result = await _userService.GetAllAsync(queryParams);
            // Trả về HTTP Status Code 200 OK kèm kết quả phân trang
            return Ok(result);
        }

        // 2. GET: api/users/{id} - Lấy User theo Id
        // MODEL BINDING: Lấy dữ liệu id từ URL (Route) truyền vào tham số `int id`
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDTO>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
            {
                // ACTION RESULT: HTTP 404 Not Found
                return NotFound(new { message = $"Không tìm thấy người dùng có Id = {id}" });
            }

            // ACTION RESULT: HTTP 200 OK kèm đối tượng user tìm thấy
            return Ok(user);
        }

        // 3. POST: api/users/register - Đăng ký tài khoản
        // MODEL BINDING: Attributes [FromBody] đọc dữ liệu JSON gửi lên ép thành đối tượng `UserCreateDTO`
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDTO>> Register([FromBody] UserCreateDTO request)
        {
            try
            {
                var createdUser = await _userService.RegisterAsync(request);
                // RESTful Standard: Trả về HTTP 201 Created cùng đường dẫn lấy tài nguyên vừa tạo
                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                // ACTION RESULT: HTTP 400 Bad Request (vd: email đã tồn tại)
                return BadRequest(new { message = ex.Message });
            }
        }

        // 4. POST: api/users/login - Đăng nhập lấy JWT Token
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] UserLoginDTO request)
        {
            var token = await _userService.LoginAsync(request);

            if (token == null)
            {
                // ACTION RESULT: HTTP 400 Bad Request
                return BadRequest(new { message = "Email hoặc mật khẩu không chính xác." });
            }

            // ACTION RESULT: HTTP 200 OK kèm JWT Token
            return Ok(new { token });
        }

        // 5. PUT: api/users/{id} - Cập nhật thông tin User
        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserResponseDTO>> Update(int id, [FromBody] UserUpdateDTO request)
        {
            var updatedUser = await _userService.UpdateAsync(id, request);

            if (updatedUser == null)
            {
                return NotFound(new { message = $"Không tìm thấy người dùng có Id = {id}" });
            }

            return Ok(updatedUser);
        }

        // 7. POST: api/users/seed?count=500 - Sinh dữ liệu mẫu (chỉ dùng cho dev/demo)
        [HttpPost("seed")]
        public async Task<IActionResult> Seed([FromQuery] int count = 500)
        {
            var inserted = await _userService.SeedRandomAsync(count);
            return Ok(new { message = $"Đã thêm {inserted} người dùng mẫu.", inserted });
        }

        // 6. DELETE: api/users/{id} - Xóa User
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _userService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new { message = $"Không tìm thấy người dùng có Id = {id}" });
            }

            // ACTION RESULT: HTTP 204 No Content
            return NoContent();
        }
    }
}
