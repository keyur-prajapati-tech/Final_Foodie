namespace Foodie.Models.customers
{
    public class tbl_cust_vendor_complaints
    {
        public int vendor_complaint_id { get; set; }
        public int restaurant_id { get; set; }
        public int customer_id { get; set; }
        public string cmp_Descr { get; set; }
        public bool cmp_Status { get; set; }
        public string ResolutionRemarks { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime ResolvedAt { get; set; }
    }
}
