using MyApiApp.DTOs;

namespace MyApiApp.Validation;

/// <summary>
/// Validate nội dung file ở tầng Service (bổ sung cho DataAnnotations ở DTO).
/// Kiểm tra: rỗng, kích thước, phần mở rộng, tên file nguy hiểm, và chữ ký file (magic bytes)
/// để chống trường hợp đổi đuôi file (vd: malware.exe -> malware.jpg).
/// </summary>
public static class FileValidator
{
    // Chữ ký (magic bytes) của các định dạng phổ biến
    private static readonly Dictionary<string, byte[][]> Signatures = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        [".gif"] = new[] { new byte[] { 0x47, 0x49, 0x46, 0x38 } },
        [".bmp"] = new[] { new byte[] { 0x42, 0x4D } },
        [".webp"] = new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } }, // RIFF....WEBP
        [".pdf"] = new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } }, // %PDF
        [".zip"] = new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 } },
        [".docx"] = new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } }, // OOXML = zip
        [".xlsx"] = new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } },
        [".pptx"] = new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } },
        [".rar"] = new[] { new byte[] { 0x52, 0x61, 0x72, 0x21 } }, // Rar!
        [".mp3"] = new[] { new byte[] { 0x49, 0x44, 0x33 }, new byte[] { 0xFF, 0xFB }, new byte[] { 0xFF, 0xF3 } },
        [".mp4"] = new[] { new byte[] { 0x00, 0x00, 0x00 } } // ftyp ở offset 4, kiểm tra riêng
    };

    // Ký tự không hợp lệ trong tên file (Windows)
    private static readonly char[] InvalidNameChars = Path.GetInvalidFileNameChars();

    /// <summary>
    /// Kiểm tra file, trả về danh sách lỗi (rỗng = hợp lệ).
    /// </summary>
    public static List<string> Validate(IFormFile? file)
    {
        var errors = new List<string>();

        if (file == null)
        {
            errors.Add("Vui lòng chọn file để upload.");
            return errors;
        }

        if (file.Length == 0)
        {
            errors.Add("File rỗng hoặc không hợp lệ.");
            return errors;
        }

        if (file.Length > FileUploadDTO.MaxFileSizeBytes)
        {
            var maxMb = FileUploadDTO.MaxFileSizeBytes / (1024.0 * 1024.0);
            errors.Add($"File vượt quá giới hạn {maxMb:0.##} MB.");
        }

        var fileName = file.FileName ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fileName))
        {
            errors.Add("Tên file không hợp lệ.");
            return errors;
        }

        // Chặn tên file chứa đường dẫn (path traversal)
        if (fileName.Contains("..") || fileName.IndexOfAny(InvalidNameChars) >= 0)
        {
            errors.Add("Tên file chứa ký tự không hợp lệ.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension) ||
            !FileUploadDTO.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"Định dạng file '{extension}' không được phép. " +
                       $"Chỉ chấp nhận: {string.Join(", ", FileUploadDTO.AllowedExtensions)}.");
            return errors;
        }

        // Kiểm tra chữ ký file (chống đổi đuôi)
        if (!HasValidSignature(file, extension))
        {
            errors.Add($"Nội dung file không khớp với định dạng '{extension}'. File có thể đã bị đổi đuôi.");
        }

        return errors;
    }

    /// <summary>
    /// Đọc vài byte đầu file để so sánh với chữ ký định dạng.
    /// Định dạng không có trong bảng (txt, csv, doc, xls, ppt, svg) thì bỏ qua kiểm tra.
    /// </summary>
    private static bool HasValidSignature(IFormFile file, string extension)
    {
        if (!Signatures.TryGetValue(extension, out var signatures))
        {
            return true; // Không có chữ ký để so sánh -> chấp nhận
        }

        // MP4: bytes 4..7 phải là "ftyp"
        if (extension == ".mp4")
        {
            var header = ReadHeader(file, 12);
            return header.Length >= 8 &&
                   header[4] == 0x66 && header[5] == 0x74 &&
                   header[6] == 0x79 && header[7] == 0x70;
        }

        var buffer = ReadHeader(file, 8);
        if (buffer.Length == 0)
        {
            return false;
        }

        return signatures.Any(sig =>
            buffer.Length >= sig.Length &&
            sig.Select((b, i) => buffer[i] == b).All(match => match));
    }

    /// <summary>
    /// Đọc tối đa <paramref name="count"/> byte đầu file mà không làm hỏng stream.
    /// </summary>
    private static byte[] ReadHeader(IFormFile file, int count)
    {
        using var stream = file.OpenReadStream();
        var buffer = new byte[count];
        var read = stream.Read(buffer, 0, count);
        return buffer.Take(read).ToArray();
    }
}
