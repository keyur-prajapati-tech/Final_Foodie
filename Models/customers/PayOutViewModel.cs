namespace Foodie.Models.customers
{
    public class PayOutViewModel
    {
        public string WeekRange { get; set; }
        public decimal OrderValue { get; set; }
        public decimal Commission { get; set; }
        public decimal GST { get; set; }
        public decimal NetPayout { get; set; }
        public string PaymentDate { get; set; }
    }
}
