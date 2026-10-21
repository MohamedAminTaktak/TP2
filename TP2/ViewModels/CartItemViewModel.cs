namespace TP2.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public string? CategoryName { get; set; }
        public decimal LineTotal => Price * Quantity;
    }
}
