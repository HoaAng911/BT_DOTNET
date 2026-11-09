using System.ComponentModel.DataAnnotations;

namespace MyApiApp.DTOs;

public class FileUploadMultipleDTO
{
    public const int MaxFileCount = 10;

    public const long MaxTotalSizeBytes = 50 * 1024 * 1024;

    [Required(ErrorMessage = "Vui lòng chọn ít nhất một file để upload.")]
    [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất một file để upload.")]
    [MaxLength(MaxFileCount, ErrorMessage = "Chỉ được upload tối đa 10 file trong một lần.")]
    public List<IFormFile> Files { get; set; } = new();

    [MaxLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }
}
