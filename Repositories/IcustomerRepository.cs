using Foodie.Models;
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

        IEnumerable<tbl_special_offers> GetAllActiveOffers();
        IEnumerable<tbl_special_offers> GetOffers();
        tbl_special_offers GetOfferById(int offerId);

        RestaurantMenuViewModel GetRestaurantMenu(int restaurantId,int? cuisineId = null);
        List<tbl_cuisine_master> GetCuisinesByRestaurantId(int restaurantId);

        //demo to display all restaurant info
        List<RestaurantCardViewModel> getAppovedOnlineRestaurants();
        //display top content in restaurant details
        //RestaurantDetailsViewModel getRestaurantDetailsByresId(int restaurantId);

        List<MenuItemViewModel> GetInspirationItems();
        MenuItemViewModel GetInspireMenuItemById(int menuid);
        List<AvailableDishViewModel> GetDeliveryFoods(int menuId);

        int PlaceOrder(RazorPayViewModel model);

        void SaveOrderItem(int orderId, OrderItemModel item);

        tbl_orders CreateOrder(int customerId, decimal grandTotal, string razorpayOrderId, List<tbl_order_items> items, int res_id, int addressId);
        void UpdateOrderStatus(string razorpayOrderId, string status);
        void SavePayment(string razorpayOrderId, string razorpayPaymentId, decimal amount, string status);

        //update quantity
        bool UpdateCartItemQuantity(int cartItemId, int newQuantity);
        tbl_cart_item GetCartItemById(int cartItemId);

        //Customer Feedback IRepository
        CustomerFeedbackViewModel GetCustomerFeedbacks(int pageNumber, int pageSize, string sortField, string sortDirection);
        int GetTotalFeedbackCountAsync();
        void InsertFeedback(tbl_customer_feedback feedback);
    }
}
