using Foodie.Models.customers;
using Foodie.Models.Restaurant;

namespace Foodie.Repositories
{
    public interface IcustomerRepository
    {
        IEnumerable<tbl_city> GetCitiesByDistrictName(string districtName);

        tbl_customer GetTbl_Customer(string email);

        void AddUser(tbl_customer tbl_Customer, byte[] profilepic);

        void UpdateCustomerProfile(tbl_customer customerinfo, byte[] profilePic);

        List<RestaurantInfo> GetAllRestaurants();

        // 🔐 Add this method
        tbl_customer ValidateCustomerLogin(string email, string password);

        /*Add To Cart Process*/
        bool MenuExists(int menuId);
        void AddToCart(tbl_cart_item cart_item);
        tbl_cart GetOrCreatecart(int customer_id);
        List<tbl_cart_item> GetCartItems(int customer_id);
        void Placeorder(int customer_id);
        tbl_menu_items GetMenuItem(int menuId);

    }
}
