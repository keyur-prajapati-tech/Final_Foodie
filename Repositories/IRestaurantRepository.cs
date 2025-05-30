using Foodie.Models;
using Foodie.Models.customers;
using Foodie.Models.Restaurant;
using Foodie.ViewModels;
using Razorpay.Api;
using System.Reflection.Metadata;

namespace Foodie.Repositories
{
    public interface IRestaurantRepository
    {

        //Restaurant 1st page
        public int AddOwner(tbl_owner_details owner);
        public int AddRestaurant(tbl_restaurant restaurant);
        //Get Data
        public tbl_restaurant getRestaurant(int id);
        public tbl_owner_details getOwner(int id);
        //Restaurant 2nd page
        public int AddVendors_img(byte[] r_img,byte[] m_img);
        public int AddVendor_Avalaiable (tbl_vendor_availability tbl_Vendor_Availability);
        public tbl_vendores_img getVendors_img(int id);
        public tbl_vendor_availability getVendor_Available(int id);
        //Restaurant 3rd page
        public int AddPanDetails(tbl_pan_details pan, byte[] img);
        public int AddGSTDetails(tbl_gst_details gst);
        public int AddFssaiDetails(tbl_fssai_Details fssai, byte[] img);
        public int AddBankDetails(tbl_bank_details bank);
        //Add Menu
        public int AddMenu(tbl_menu_items menu);
        public List<tbl_menu_items> getMenuByRes(int id);
        //delete menu
        public int DeleteMenu(int id);
        public tbl_menu_items getMenu(int id);
        public int UpdateMenu(tbl_menu_items menu);
        //Order Notification
        public List<tbl_orders_notifi> tbl_Orders_Notifis(int restaurant_id);
        //Accept Order by Resturant
        public int AcceptOrder(int order_id,string food_status);
        public int OrderReady(int order_id,int restaurant_id);
        //get all Order with Accepted status
        public List<ordersViewMdel> tbl_Orders_Notifis_Accepted(int restaurant_id);
        public List<ordersViewMdel> tbl_Orders_History(int restaurant_id);
        //isOnline
        public int IsOnline(int restaurant_id, int isOnline);
        public int getOnline(int restaurant_id);
        public bool isApprove(int restaurant_id);
        public OutletInfo GetOutletInfo(int id);
        public void UpdateOutletInfo(OutletInfo model);
        public List<tbl_ratings> GetAllRatings(int restaurant_id);
        public IEnumerable<tbl_cust_vendor_complaints> GetComplaintsByRestaurantId(int restaurantId);
        public void updateVencom(tbl_cust_vendor_complaints tbl_Cust_Vendor_Complaints);
        
        //Offer Section IRepo
        IEnumerable<tbl_special_offers> GetAllOffers();
        void AddOffeer(tbl_special_offers offers);
        tbl_special_offers GetOfferById(int id);
        void UpdateOffer(tbl_special_offers offer);
        void DeleteOffer(int id);
        IEnumerable<tbl_special_offers> GetOffersByDateRange(DateTime? validFrom, DateTime? validTo);
        IEnumerable<tbl_special_offers> GetOffersByStatus(bool isActive);
        IEnumerable<tbl_special_offers> GetOffersByDateAndStatus(DateTime? validFrom, DateTime? validTo, bool? isActive);


        bool ValidateOTP(string email, string otp);
        void SaveOTP(string email, string otp);
        public PayoutsDetailsViewModel GetBankDetailsByRestaurantId(int restaurantId);
        public List<OrderViewModel> GetWeeklyOrderStatsAsync(int restaurantid);
        public List<OrderViewModel> GetWeeklyCustomerRatings(int restaurantid);
        List<reports> GetPerformanceMetrics();


        void InsertFeedback(tbl_vendor_feedback feedback);
        List<tbl_vendor_feedback> GetAllFeedback();


        //payouts Details Display
        List<PayoutsDetailsViewModel> GetWeeklySalesByMonth(int year, int month, int resId);
        IEnumerable<PayOutViewModel> GetWeeklyPayouts(PayoutfilterModel filter);
        
        // Add these new methods for exports
        byte[] GenerateWeeklyPayoutsPdf(PayoutfilterModel filter);
        byte[] GenerateWeeklyPayoutsExcel(PayoutfilterModel filter);


        //Earning Summary Irepo
        EarningSummaryViewModel GetEarningsSummary();
        IEnumerable<tbl_orders> GetOrders();
        IEnumerable<payments> GetPayments();
        
    }
}
