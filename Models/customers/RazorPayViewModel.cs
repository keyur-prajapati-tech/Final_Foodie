namespace Foodie.Models.customers
{
    public class RazorPayViewModel
    {
        public int customer_id { get; set; }
        public int? coupone_id { get; set; }
        public int AddressId { get; set; }
        public int PaymentModeId { get; set; }
        public decimal grandtotal { get; set; }
        public decimal discount_amount { get; set; }
        public int restaurant_id { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpayOrderId { get; set; }

        public List<OrderItemModel> Items { get; set; }
    }
}
