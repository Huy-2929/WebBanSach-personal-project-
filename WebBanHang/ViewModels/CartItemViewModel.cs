namespace WebBanHang.ViewModels
{
    public class CartItemViewModel
    {
        public int BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }

        public decimal SubTotal => Price * Quantity;
    }
}
