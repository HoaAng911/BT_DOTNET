using System.ComponentModel.DataAnnotations;

namespace MyApiApp.Models;

/// <summary>
/// </summary>
public class FileUpload
{
    public int Id { get; set; }

    
    [Required]
    [MaxLength(255)]
    public string OriginalName { get; set; } = string.Empty;

    // Tên file thực tế lưu trên đĩa (guid + extension), dùng để truy cập/xóa
    [Required]
    [MaxLength(255)]
    public string StoredName { get; set; } = string.Empty;

    // Đường dẫn tương đối để truy cập file (vd: /uploads/abc.png)
    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    // Kích thước file (byte)
    public long Size { get; set; }

    // MIME type
    [MaxLength(150)]
    public string ContentType { get; set; } = string.Empty;

    // Phần mở rộng (vd: .png)
    [MaxLength(20)]
    public string Extension { get; set; } = string.Empty;

    // Mô tả kèm theo
    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
