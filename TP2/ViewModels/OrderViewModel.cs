using System.Collections.Generic;

namespace TP2.ViewModels
{
    public class OrderViewModel
    {
        public string Address { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public float TotalAmount { get; set; }
        public List<CartItemViewModel> CartItems { get; set; } = new();
    }
}
