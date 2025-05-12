namespace Foodie.Models.customers
{
    public class RazorCheckoutPageViewModel
    {
        public int CustomerId { get; set; }
        public int AddressId { get; set; }
        public int PaymentModeId { get; set; }
        public int? CouponId { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal DiscountAmount { get; set; }

        public List<CartItemViewModel> CartItems { get; set; }

        public List<CartItemViewModel> cartItems { get; set; }
    }
}
