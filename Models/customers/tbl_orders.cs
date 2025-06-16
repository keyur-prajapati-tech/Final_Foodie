using Foodie.Models.Restaurant;
using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.customers
{
    public class tbl_orders
    {
        public int order_id { get; set; }
        public int customer_id { get; set; }
        public DateTime order_date { get; set; } = DateTime.Now;
        public string order_status { get; set; } = "Pending";
        public decimal grand_total { get; set; }
        public string razorpay_payment_id { get; set; }
        public string razorpay_order_id { get; set; }
        public string razorpay_signature { get; set; }
        public int address_id { get; set; }

        public string customer_name { get; set; }
        public string delivery_address { get; set; }
        public int restaurant_id { get; set; }
        public string customer_phone { get; set; }
        public List<tbl_order_items> OrderItems { get; set; } = new List<tbl_order_items>();
        public List<tbl_menu_items> MenuItems { get; set; } = new List<tbl_menu_items>();

        public int ItemCount { get; set; }
    }
}
