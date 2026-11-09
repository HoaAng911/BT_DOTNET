using Microsoft.EntityFrameworkCore;
using MyApiApp.Models;

namespace MyApiApp.Data
{
  public class AppDbContext : DbContext
  {
    // Constructor nhận DbContextOptions để truyền cấu hình (chuỗi kết nối, provider...) từ Program.cs
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ---------- DbSet ----------
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<FileUpload> FileUploads { get; set; }

    // Cấu hình quan hệ, index, ràng buộc bổ sung (Fluent API)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // User: Email là duy nhất
      modelBuilder.Entity<User>()
        .HasIndex(u => u.Email)
        .IsUnique();

      // Category: Name là duy nhất
      modelBuilder.Entity<Category>()
        .HasIndex(c => c.Name)
        .IsUnique();

      // Category 1 - N Product
      modelBuilder.Entity<Product>()
        .HasOne(p => p.Category)
        .WithMany(c => c.Products)
        .HasForeignKey(p => p.CategoryId)
        .OnDelete(DeleteBehavior.Restrict); // Không xóa Product khi xóa Category

      // User 1 - N Order
      modelBuilder.Entity<Order>()
        .HasOne(o => o.User)
        .WithMany()
        .HasForeignKey(o => o.UserId)
        .OnDelete(DeleteBehavior.Restrict); // Không xóa Order khi xóa User

      // Order: OrderCode là duy nhất
      modelBuilder.Entity<Order>()
        .HasIndex(o => o.OrderCode)
        .IsUnique();

      // Order 1 - N OrderItem
      modelBuilder.Entity<OrderItem>()
        .HasOne(oi => oi.Order)
        .WithMany(o => o.OrderItems)
        .HasForeignKey(oi => oi.OrderId)
        .OnDelete(DeleteBehavior.Cascade); // Xóa Order thì xóa luôn OrderItem

      // Product 1 - N OrderItem
      modelBuilder.Entity<OrderItem>()
        .HasOne(oi => oi.Product)
        .WithMany(p => p.OrderItems)
        .HasForeignKey(oi => oi.ProductId)
        .OnDelete(DeleteBehavior.Restrict); // Không xóa Product khi xóa OrderItem

      // FileUpload: StoredName là duy nhất (mỗi file trên đĩa chỉ có 1 bản ghi)
      modelBuilder.Entity<FileUpload>()
        .HasIndex(f => f.StoredName)
        .IsUnique();
    }
  }
}