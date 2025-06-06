﻿using Foodie.Models;
using Foodie.Models.Admin;
using Foodie.Models.customers;
using Foodie.Models.Restaurant;
using Foodie.ViewModels;

namespace Foodie.Repositories
{
    public interface IAdminRepository
    {

        //Admin

        tbl_admin GetAdminById(int adminId);
        void UpdateAdmin(tbl_admin admin);

        List<tbl_admin> GetAll();
        bool Login(string Email, string Password);
        bool AddAdmin(tbl_admin admin, byte[] Image);
        tbl_admin getSessionData(string Email);

        //Role

        IEnumerable<tbl_roles> GetRole();

        //feedback 

        public IEnumerable<tbl_vendor_feedback> GetAllVendorFeedback();
        public IEnumerable<tbl_customer_feedback> GetAllCustomerFeedback();
        public IEnumerable<tbl_delivery_feedback> GetAllDeliveryFeedback();

        //Complaints
        public IEnumerable<tbl_vendor_complaints> GetAllVendorComplaints();
        void updateVencom(tbl_vendor_complaints tbl_Vendor_Complaints);
        public IEnumerable<tbl_partner_complaints> GetAllDeliveryComplaints();
        void updateDelcom(tbl_partner_complaints tbl_Partner_Complaints);
        public IEnumerable<tbl_customer_complaints> GetAllCustomerComplaints();

        List<tbl_cust_vendor_complaints> GetcustVendorComplaints();
        List<tbl_cust_partner_complaints> GetcustPartnerComplaints();


        //Restaurants
        public IEnumerable<tbl_restaurant> GetAllRestaurant();

        //Customers
        public IEnumerable<tbl_customer> GetAllCustomer();


        //Dashboard

        decimal GetMonthlySales();
        int GetMonthlyCustomerCount();
        int GetMonthlyRestaurantCount();

        int GetCancelledOrders();
        int GetPendingOrders();
        int GetAcceptedOrders();
        int GetDeliveredOrders();

        int GetActiveRestaurants();
        int GetInactiveRestaurants();
        int GetOpenRestaurants();
        int GetClosedRestaurants();

      



        List<DashBoardViewModel> GetWeeklySales(int year, int month);
        List<DashBoardViewModel> GetYearlySales(int year);
        //orders

        List<OrderViewModel> GetAllOrders(string status);




        List<VendoreViewModel> GetVendorsForApproval();
        VendoreViewModel GetVendorById(int restaurantId);
        void UpdateApprovalStatus(int restaurantId, bool isApproved);

        public bool DeleteVendor(int id);



        public IEnumerable<tbl_restaurant> GetPendingRestaurant();









        public int AddCuisine(string cuisineName);
        public List<tbl_cuisine_master> GetAllCuisines();
        public void UpdateCuisine(int id, string name);
        public void DeleteCuisine(int id);

        public List<OrderViewModel> GetFilteredOrders(DateTime? fromDate, DateTime? toDate, string orderStatus);


        bool UpdatePassword(string email, string newPassword);

        public bool ChangeResStatus(int resid, bool status);
    }
}
