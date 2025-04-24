namespace Foodie.Models
{
    public class tbl_customer_complaints
    {
       public int customer_complaint_id { get; set; }
	   public int customer_id { get; set; }
       public string customer_name { get; set; }
       public string cmp_Descr { get; set; }
	   public int cmp_Status { get; set; }
	   public int admin_id { get; set; }
       public string Full_name { get; set; }
       public int restaurant_id { get; set; }
       public string restaurant_name { get; set; }
       public string ResolutionRemarks { get; set; }
       public DateTime createdAt { get; set; }
       public DateTime ResolvedAt { get; set; }
    }
}
