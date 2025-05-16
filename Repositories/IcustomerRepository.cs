using Foodie.Models.Admin;
using Foodie.Models.customers;
using Foodie.Models.Restaurant;
using static Foodie.Models.customers.tbl_coupone;

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
        public tbl_restaurant ValidateRestaurantLogin(string email, string password);
        List<tbl_menu_items> GetAllMenuItems();
        IEnumerable<tbl_menu_items> GetMenuItemsByRestaurant(int restaurantId);



        List<CartItemViewModel> GetCartItems(int customerId);
        void IncreaseQuantity(int cartItemId);
        void DecreaseQuantity(int cartItemId);
        void RemoveCartItem(int cartItemId);
        decimal GetCartTotal(int customerId);

        void AddToCart(tbl_cart_item tbl_Cart_Item);
        tbl_cart GetOrCreateCart(int customerId);

        IEnumerable<tbl_state> GetAllStates();
        IEnumerable<tbl_district> GetDistrictByStateId(int stateId);
        IEnumerable<tbl_city> GetCitiesByDistrictId(int districtId);
        void AddAddress(tbl_address tbl_Address);


        tbl_menu_items GetMenuItemById(int id);

        tbl_customer GetCustomerNameAndPhone(int customerId);

        List<tbl_coupone> GetAllCoupons();
        tbl_coupone GetAutoApplicableCoupon(decimal grandTotal);

        decimal CalculateGrandTotal(int customer_id);

        int PlaceOrder(int customer_id, int? coupone_id, int address_id, int paymentModeId, decimal totalAmount, decimal discount, string razorpayPaymentId, string razorpayOrderId);

        void SaveOrderItem(int orderId, int menuId, int quantity, decimal price, decimal discount);

        IEnumerable<tbl_special_offers> GetAllActiveOffers();
        IEnumerable<tbl_special_offers> GetOffers();
        tbl_special_offers GetOfferById(int offerId);

        RestaurantMenuViewModel GetRestaurantMenu(int restaurantId,int? cuisineId = null);
        List<tbl_cuisine_master> GetCuisinesByRestaurantId(int restaurantId);

        //demo to display all restaurant info
        List<RestaurantCardViewModel> getAppovedOnlineRestaurants();
        //display top content in restaurant details
        RestaurantMenuViewModel getRestaurantDetails(int restaurantId);
    }
}
