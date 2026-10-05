using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebBanHang.ViewModels
{
    public class BookViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sách")]
        [StringLength(250, ErrorMessage = "Tên sách không quá 250 ký tự")]
        [Display(Name = "Tên sách")]
        public string BookName { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "ISBN không quá 20 ký tự")]
        [Display(Name = "Mã ISBN")]
        public string? ISBN { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(0, 1000000000, ErrorMessage = "Giá sách phải từ 0 trở lên")]
        [Display(Name = "Giá bán (VNĐ)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(0, 100000, ErrorMessage = "Số lượng phải từ 0 trở lên")]
        [Display(Name = "Số lượng tồn kho")]
        public int Quantity { get; set; }

        [Display(Name = "Mô tả sách")]
        public string? Description { get; set; }

        [Display(Name = "Ảnh bìa sách")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Tải ảnh mới")]
        public IFormFile? ImageFile { get; set; }

        [Range(1000, 2100, ErrorMessage = "Năm xuất bản không hợp lệ")]
        [Display(Name = "Năm xuất bản")]
        public int? PublishedYear { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tác giả")]
        [Display(Name = "Tác giả")]
        public int AuthorId { get; set; }
        public string? AuthorName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhà xuất bản")]
        [Display(Name = "Nhà xuất bản")]
        public int PublisherId { get; set; }
        public string? PublisherName { get; set; }
    }
}
