using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApiApp.Models;

public class OrderItem
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
    public int Quantity { get; set; }

    // Lưu giá tại thời điểm mua để không bị thay đổi khi Product đổi giá
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    // FK: N OrderItem - 1 Order
    public int OrderId { get; set; }

    public Order? Order { get; set; }

    // FK: N OrderItem - 1 Product
    public int ProductId { get; set; }

    public Product? Product { get; set; }
}