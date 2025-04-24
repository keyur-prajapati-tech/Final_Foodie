using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Primitives;
using System.Security.Cryptography.Pkcs;

namespace Foodie.Models
{
    public class tbl_vendor_complaints
    {
        public int vendor_complaint_id { get; set; }
        public int restaurant_id { get; set; }
        public string restaurant_name { get; set; }
        public string cmp_Descr { get; set; }
        public bool cmp_Status { get; set; }
        public int admin_id { get; set; }
        public string Full_name { get; set; }
        public string ResolutionRemarks {get;set;}
        public DateTime	createdAt { get; set; }
        public DateTime ResolvedAt { get; set; }
    }
}
