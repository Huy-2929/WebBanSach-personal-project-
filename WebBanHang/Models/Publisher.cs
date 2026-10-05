using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Models
{
    [Table("Publishers")]
    public class Publisher
    {
        [Key]
        public int PublisherId { get; set; }

        [Required(ErrorMessage = "Tên nhà xuất bản là bắt buộc")]
        [StringLength(150, ErrorMessage = "Tên nhà xuất bản không vượt quá 150 ký tự")]
        public string PublisherName { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Địa chỉ không vượt quá 255 ký tự")]
        public string? Address { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại không vượt quá 20 ký tự")]
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
