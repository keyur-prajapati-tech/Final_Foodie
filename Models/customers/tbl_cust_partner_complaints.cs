namespace Foodie.Models.customers
{
    public class tbl_cust_partner_complaints
    {
        public int partner_complaint_id { get; set; }
        public int partner_id { get; set; }
        public int customer_id { get; set; }
        public string cmp_Descr { get; set; }
        public bool cmp_Status { get; set; }
        public string ResolutionRemarks { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime ResolvedAt { get; set; }
    }
}
