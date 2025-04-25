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

        List<tbl_menu_items> GetAllMenuItems();
        tbl_menu_items GetMenuItemById(int id);
        void AddToCart(tbl_cart_item tbl_Cart_Item);
        tbl_cart GetOrCreateCart(int customerId);
        List<tbl_cart_item> GetCartItems(int customerId);
        void PlaceOrder(int customerId);
    }
}
