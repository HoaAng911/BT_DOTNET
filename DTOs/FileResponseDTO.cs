namespace MyApiApp.DTOs;

public class FileResponseDTO
{
    public int Id { get; set; }

    public string OriginalName { get; set; } = string.Empty;

    public string StoredName { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public long Size { get; set; }

    public string SizeDisplay { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime UploadedAt { get; set; }
}
