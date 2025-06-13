namespace Foodie.Models.customers
{
    public class UserOrder
    {
        public int order_id { get; set; }
        public string order_status { get; set; }
        public string AssignmentStatus { get; set; }
        public DateTime order_date { get; set; }
        public DateTime? delivered_date { get; set; }
        public string customer_name { get; set; }
        public decimal grand_total { get; set; }
        public string delivery_address { get; set; }
        public string restaurant_name { get; set; }
        public string restaurant_image { get; set; }
        public string restaurant_street { get; set; }
        public string restaurant_pincode { get; set; }
    }
}
