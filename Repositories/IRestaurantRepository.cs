using Foodie.Models.Restaurant;
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

        //Restaurant 3rd page
        public int AddPanDetails(tbl_pan_details pan, byte[] img);
        public int AddGSTDetails(tbl_gst_details gst);
        public int AddFssaiDetails(tbl_fssai_Details fssai, byte[] img);
        public int AddBankDetails(tbl_bank_details bank);


    }
}
