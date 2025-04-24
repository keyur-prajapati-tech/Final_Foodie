using Foodie.Models;
using Foodie.Models.Restaurant;
using Foodie.ViewModels;

namespace Foodie.Repositories
{
    public interface IAdminRepository
    {
        List<tbl_admin> GetAll();
       
        bool Login(string Email, string Password);
        bool AddAdmin(tbl_admin admin ,byte[] Image);
        tbl_admin getSessionData(string Email);
        IEnumerable<tbl_roles> GetRole();
        public IEnumerable<tbl_vendor_feedback> GetAllVendorFeedback();
        public IEnumerable<tbl_customer_feedback> GetAllCustomerFeedback();
        public IEnumerable<tbl_delivery_feedback> GetAllDeliveryFeedback();
        public IEnumerable<tbl_vendor_complaints> GetAllVendorComplaints();
        void updateVencom(tbl_vendor_complaints tbl_Vendor_Complaints);
        public IEnumerable<tbl_partner_complaints> GetAllDeliveryComplaints();
        void updateDelcom(tbl_partner_complaints tbl_Partner_Complaints);
        public IEnumerable<tbl_customer_complaints> GetAllCustomerComplaints();


        public IEnumerable<tbl_restaurant> GetAllRestaurant();
    }
}
