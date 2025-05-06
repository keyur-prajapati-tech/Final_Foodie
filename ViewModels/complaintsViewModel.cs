using Foodie.Models;
using Foodie.Models.customers;

namespace Foodie.ViewModels
{
    public class complaintsViewModel
    {

        public IEnumerable<tbl_customer_complaints> tbl_customer_complaints { get; set; }
        public IEnumerable<tbl_partner_complaints> tbl_partner_complaints { get; set; }
        public IEnumerable<tbl_vendor_complaints> tbl_Vendor_Complaints { get; set; }

        public tbl_vendor_complaints? tbl_Vendor_Complaint { get; set; }
        public tbl_partner_complaints? tbl_Partner_Complaints { get; set; }


        public List<tbl_cust_vendor_complaints> tbl_cust_vendor_complaints { get; set; }
        public List<tbl_cust_partner_complaints> tbl_cust_partner_complaints { get; set; }

    }
}