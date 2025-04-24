namespace Foodie.Models
{
    public class tbl_vendor_feedback
    {
        public int vendore_feedback_id { get; set; }
        public int restaurant_id { get; set; }
        public string restaurant_name { get; set; }
        public decimal rating { get; set; }
        public string feedback_description { get; set; }
        public DateTime createdAt { get; set; }
    }
}
