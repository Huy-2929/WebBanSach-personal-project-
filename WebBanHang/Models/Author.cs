using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Models
{
    [Table("Authors")]
    public class Author
    {
        [Key]
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "Tên tác giả là bắt buộc")]
        [StringLength(150, ErrorMessage = "Tên tác giả không vượt quá 150 ký tự")]
        public string AuthorName { get; set; } = string.Empty;

        public string? Biography { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
