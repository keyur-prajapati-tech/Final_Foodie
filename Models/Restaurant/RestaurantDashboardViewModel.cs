using Foodie.Models.Admin;
using Foodie.Models.customers;

namespace Foodie.Models.Restaurant
{
    public class RestaurantDashboardViewModel
    {
        public DashboardStats Stats { get; set; }
        public List<tbl_orders> RecentOrders { get; set; }
        public List<CuisineStats> CuisineStats { get; set; }
        public List<PopularItem_ViewModel> PopularItems { get; set; }
        public List<tbl_cuisine_master> Cuisines { get; set; }
    }
}
