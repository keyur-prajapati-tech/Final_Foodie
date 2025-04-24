using Foodie.Models;

namespace Foodie.ViewModels
{
    public class feedbackViewModel
    {
        public IEnumerable<tbl_vendor_feedback> tbl_Vendor_Feedback { get; set; }
        public IEnumerable<tbl_customer_feedback> tbl_customer_Feedback { get; set; }
        public IEnumerable<tbl_delivery_feedback> tbl_Delivery_Feedback { get; set; }
    }
}
