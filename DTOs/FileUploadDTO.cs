using System.ComponentModel.DataAnnotations;
using MyApiApp.Validation;

namespace MyApiApp.DTOs;

/// <summary>
/// DTO nhận dữ liệu upload file từ client (multipart/form-data).
/// Validation được thực hiện tự động bởi [ApiController] trước khi vào action.
/// </summary>
public class FileUploadDTO
{
    // Giới hạn 10 MB (khớp với RequestSizeLimit ở Controller)
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;

    // Danh sách phần mở rộng cho phép (dùng chung cho cả validate và thông báo lỗi)
    public static readonly string[] AllowedExtensions =
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".zip", ".rar", ".mp3", ".mp4"
    };

    // [Required]: Bắt buộc phải có file gửi lên
    [Required(ErrorMessage = "Vui lòng chọn file để upload.")]
    [MaxFileSize(MaxFileSizeBytes, ErrorMessage = "File vượt quá giới hạn 10 MB.")]
    [AllowedFileExtensions(
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".zip", ".rar", ".mp3", ".mp4")]
    public IFormFile File { get; set; } = default!;

    // Mô tả kèm theo file (không bắt buộc)
    [MaxLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }
}
