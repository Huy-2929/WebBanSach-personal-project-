using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Models
{
    [Table("Books")]
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Tên sách là bắt buộc")]
        [StringLength(250, ErrorMessage = "Tên sách không vượt quá 250 ký tự")]
        public string BookName { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "ISBN không vượt quá 20 ký tự")]
        public string? ISBN { get; set; }

        [Required(ErrorMessage = "Giá bán là bắt buộc")]
        [Range(0, 1000000000, ErrorMessage = "Giá sách phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, 100000, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int Quantity { get; set; } = 0;

        public string? Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Range(1000, 2100, ErrorMessage = "Năm xuất bản không hợp lệ")]
        public int? PublishedYear { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tác giả")]
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhà xuất bản")]
        public int PublisherId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("AuthorId")]
        public virtual Author? Author { get; set; }

        [ForeignKey("PublisherId")]
        public virtual Publisher? Publisher { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
