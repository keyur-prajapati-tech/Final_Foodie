using System.Security.Cryptography.Pkcs;

namespace Foodie.Models
{
    public class tbl_partner_complaints
    {
       public int partner_id { get; set; }
       public int partner_complaint_id { get; set; }
       public string Fullname { get; set; }
       public string cmp_Descr { get; set; }
	   public bool cmp_Status { get; set; }
	   public int admin_id { get; set; }
       public string Full_name { get; set; }
       public string ResolutionRemarks {get;set;}
       public DateTime createdAt { get; set; }
       public DateTime ResolvedAt { get; set; }
    }
}
