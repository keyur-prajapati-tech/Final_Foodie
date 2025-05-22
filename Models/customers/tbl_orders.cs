using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.customers
{
    public class tbl_orders
    {
        public int order_id { get; set; }
        public int customer_id { get; set; }
        public DateTime order_date { get; set; } = DateTime.Now;
        public string order_status { get; set; } = "Pending";
        public int copone_id { get; set; }
        public string food_status { get; set; } = "Processing";
        public int address_id { get; set; }
        public int restaurant_id { get; set; }
        public List<tbl_order_items> OrderItems { get; set; } = new List<tbl_order_items>();
        public int razorepay_payment_id { get; set; }
        public string razorpay_order_id { get; set; }
        public string razorpay_signature { get; set; }
        public decimal grand_total { get; set; }
        public decimal discount_amount { get; set; }
    }
}
