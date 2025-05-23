namespace Foodie.Models.customers
{
    public class payments
    {
        //payments table
        public int payment_id { get; set; }
        public int order_id { get; set; }
        public int razorepay_payment_id { get; set; }
        public decimal amount { get; set; }
        public string payment_status { get; set; }
        public DateTime payment_date { get; set; }
    }
}
