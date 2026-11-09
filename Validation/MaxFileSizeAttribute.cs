using System.ComponentModel.DataAnnotations;

namespace MyApiApp.Validation;

/// <summary>
/// Attribute kiểm tra kích thước IFormFile không vượt quá giới hạn (tính bằng byte).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly long _maxBytes;

    public MaxFileSizeAttribute(long maxBytes)
    {
        _maxBytes = maxBytes;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Không có file thì để [Required] xử lý
        if (value is not IFormFile file || file.Length == 0)
        {
            return ValidationResult.Success;
        }

        if (file.Length > _maxBytes)
        {
            var maxMb = _maxBytes / (1024.0 * 1024.0);
            return new ValidationResult(
                $"File vượt quá giới hạn {maxMb:0.##} MB (file hiện tại {file.Length / (1024.0 * 1024.0):0.##} MB).",
                new[] { validationContext.MemberName ?? "File" });
        }

        return ValidationResult.Success;
    }
}
