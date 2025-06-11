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
        RestaurantInfo GetRestaurantByName(string restaurantName);
        List<MenuItemViewModel> GetMenuItemsByRestaurantdetails(int restaurantId);
        List<RestaurantInfo> GetTopRestaurant(int count = 5);
        List<MenuItemViewModel> getCuisinrByRestaurant(int restaurantId);
        //List<MenuItemViewModel> GetMenuItemByRestaurantAndCuisine(int restaurantId, int cuisineId);


        // 🔐 Add this method
        tbl_customer ValidateCustomerLogin(string email, string password);
        public tbl_restaurant ValidateRestaurantLogin(string email, string password);
        List<tbl_menu_items> GetAllMenuItems();



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



        tbl_customer GetCustomerNameAndPhone(int customerId);

        List<tbl_coupone> GetAllCoupons();
        tbl_coupone GetAutoApplicableCoupon(decimal grandTotal);

        decimal CalculateGrandTotal(int customer_id);

        IEnumerable<tbl_special_offers> GetAllActiveOffers();
        IEnumerable<tbl_special_offers> GetOffers();
        tbl_special_offers GetOfferById(int offerId);

        List<tbl_cuisine_master> GetCuisinesByRestaurantId(int restaurantId);

        //demo to display all restaurant info
        List<RestaurantCardViewModel> getAppovedOnlineRestaurants();
        //display top content in restaurant details
        //RestaurantDetailsViewModel getRestaurantDetailsByresId(int restaurantId);

        List<MenuItemViewModel> GetInspirationItems();
        MenuItemViewModel GetInspireMenuItemById(int menuid);
        List<MenuItemViewModel> GetDeliveryItemsByOrderId(int orderId);

        List<AvailableDishViewModel> GetDeliveryFoods(int menuId);

        int PlaceOrder(RazorPayViewModel model);

        void SaveOrderItem(int orderId, OrderItemModel item);

        tbl_orders CreateOrder(int customerId, decimal grandTotal, string razorpayOrderId, List<tbl_order_items> items, int res_id, int addressId);
        void UpdateOrderStatus(string razorpayOrderId, string status);
        void SavePayment(string razorpayOrderId, string razorpayPaymentId, decimal amount, string status);

        //update quantity
        bool UpdateCartItemQuantity(int cartItemId, int newQuantity);
        tbl_cart_item GetCartItemById(int cartItemId);
        bool ClearCustomerCart(int customerId);

        //Customer Feedback IRepository
        CustomerFeedbackViewModel GetCustomerFeedbacks(int pageNumber, int pageSize, string sortField, string sortDirection);
        int GetTotalFeedbackCountAsync();
        void InsertFeedback(tbl_customer_feedback feedback);
        List<tbl_restaurant> GetApprovedRestaurants();

        //Cuisine Wise Menu Filter
        IEnumerable<tbl_menu_items> GetMenuItemsByRestaurant(int restaurantId);
        tbl_menu_items GetMenuItemById(int id);
        RestaurantMenuViewModel GetRestaurantMenu(int restaurantId,int? cuisineId = null);

        // Add these new methods for cuisine filtering
        IEnumerable<tbl_cuisine_master> GetCuisinesByRestaurant(int restaurantId);
        IEnumerable<MenuItemViewModel> GetMenuItemsByRestaurantAndCuisine(int restaurantId, int cuisineId);



        MenuItemViewModel GetMenuViewModelAsync(int restaurantId, int? cuisineId = null);
        List<tbl_cuisine_master> GetCuisinesWithCountAsync(int restaurantId);

        List<SimilarRestaurantDtoViewModel> GetSimilarRestaurants(
        string currentRestaurantName,
        List<string> cuisines,
        List<string> pincodes);

        //Menu Image based on restaurantname
        List<MenuItemViewModel> GetMenuItemsImageByRestaurant(string restaurantName);
        List<tbl_cuisine_master> GetAllCuisines();

        List<MenuItemViewModel> GetMenuItems(string restaurantName);
        List<MenuItemViewModel> GetMenuItemsByCuisine(string restaurantName, int cuisineId);

        /*Rating Section Irepository here*/
        List<CustomerRatingViewModel> GetRatingByRestaurant(string restaurantName);
        bool AddRating(CustomerRatingViewModel model);
        bool UpdateRating(CustomerRatingViewModel model);
        CustomerRatingViewModel GetRatingById(int ratingId, int customerId);
        void ClearCartAfterOrder(int customer_id);

        /*All Deliverd Food Will be display IRepo*/
        List<RestaurantViewModel> GetRecommendedRestaurants(int count = 6);
        List<RestaurantViewModel> GetRestaurantsByCuisine(int cuisineId);
        List<RestaurantViewModel> SearchRestaurants(string searchTerm);

        //ORDER HISTORY
        List<tbl_orders> GetUserOrdersWithItemsAndImages(int userId);
        bool SubmitReview(tbl_ratings ratings);
        byte[] GenerateBill(int orderId); //Restaurant is pendding
        List<tbl_orders> FilterOrders(int userId, string status, int? days);

        IEnumerable<TopSellingMenuViewModel> topSellingMenuViewModels(int restaurantId, int count = 5);
    }
}
