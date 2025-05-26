namespace Foodie.Models
{
    public class tbl_customer_feedback
    {
        public int cust_feedback_id { get; set; }
        public int customer_id { get; set; }
        public int restaurant_id { get; set; }
        public decimal rating { get; set; }
        public string customer_name { get; set; }
        public string restaurant_name { get; set; }
        public string feedback_description { get; set; }
        public DateTime createdAt { get; set; }
    }
}
