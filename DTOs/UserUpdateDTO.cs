using System.ComponentModel.DataAnnotations;

namespace MyApiApp.DTOs;

public class UserUpdateDTO
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    [MinLength(6)]
    public string? Password { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }
}