using System.ComponentModel.DataAnnotations;

namespace WebBanHang.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        [StringLength(100, ErrorMessage = "Họ tên không vượt quá 100 ký tự")]
        [Display(Name = "Họ tên người nhận")]
        public string ShippingName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không vượt quá 20 ký tự")]
        [Display(Name = "Số điện thoại nhận hàng")]
        public string ShippingPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [StringLength(255, ErrorMessage = "Địa chỉ không vượt quá 255 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Ghi chú không vượt quá 500 ký tự")]
        [Display(Name = "Ghi chú đơn hàng")]
        public string? Note { get; set; }

        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

        public decimal TotalAmount => CartItems.Sum(x => x.SubTotal);
    }
}
