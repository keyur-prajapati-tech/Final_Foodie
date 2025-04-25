using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.customers
{
    public class tbl_orders
    {
        [Key]
        public int order_id { get; set; }
        public int customer_id { get; set; }
        public DateTime order_date { get; set; }
        public string order_status { get; set; }
        public int coupone_id { get; set; }
        public DateTime created_at { get; set; }
        public string food_status { get; set; }
        public int payment_mode_id { get; set; }
        public int address_id { get; set; }
        public int restaurant_id { get; set; }
    }
}
