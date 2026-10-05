using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không vượt quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150, ErrorMessage = "Email không vượt quá 150 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Số điện thoại không vượt quá 20 ký tự")]
        public string? Phone { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ không vượt quá 255 ký tự")]
        public string? Address { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Customer"; // Customer or Admin

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
