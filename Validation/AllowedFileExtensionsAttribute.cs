using System.ComponentModel.DataAnnotations;

namespace MyApiApp.Validation;

/// <summary>
/// Attribute kiểm tra phần mở rộng của IFormFile có nằm trong danh sách cho phép.
/// Dùng trên property kiểu IFormFile trong DTO.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class AllowedFileExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedFileExtensionsAttribute(params string[] extensions)
    {
        // Chuẩn hóa: luôn có dấu chấm và chữ thường
        _extensions = extensions
            .Select(e => e.StartsWith('.') ? e.ToLowerInvariant() : "." + e.ToLowerInvariant())
            .ToArray();
    }

    public IReadOnlyList<string> Extensions => _extensions;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Không có file thì để [Required] xử lý
        if (value is not IFormFile file || file.Length == 0)
        {
            return ValidationResult.Success;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension) || !_extensions.Contains(extension))
        {
            return new ValidationResult(
                $"Định dạng file '{extension}' không được phép. Chỉ chấp nhận: {string.Join(", ", _extensions)}.",
                new[] { validationContext.MemberName ?? "File" });
        }

        return ValidationResult.Success;
    }
}
