using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApiApp.Models;

public class Order
{
    public int Id { get; set; }

    // Mã đơn hàng hiển thị cho khách (vd: ORD-20260921-0001)
    [Required]
    [MaxLength(50)]
    public string OrderCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ShippingAddress { get; set; }

    // Trạng thái: Pending | Confirmed | Shipping | Completed | Cancelled
    [MaxLength(30)]
    public string Status { get; set; } = "Pending";

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // FK: N Order - 1 User
    public int UserId { get; set; }

    public User? User { get; set; }

    // 1 Order - N OrderItem
    public List<OrderItem> OrderItems { get; set; } = new();
}