namespace Foodie.Models
{
    public class tbl_delivery_feedback
    {
        public int delivery_feedback_id { get; set; }
        public int partner_id { get; set; }
        public string Fullname { get; set; }
        public decimal rating { get; set; }
        public string feedback_description { get; set; }
        public DateTime createdAt { get; set; }
    }
}
