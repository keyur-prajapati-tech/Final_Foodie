namespace Foodie.Models.customers
{
    public class CouponApplicationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal FinalTotal { get; set; }
    }
}
