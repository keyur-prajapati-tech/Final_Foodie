using Foodie.Models.Admin;

namespace Foodie.Models.customers
{
    public class RestaurantMenuViewModel
    {
        public int restauranr_id { get; set; }
        public string restaurant_name { get; set; }

        public List<tbl_cuisine_master> Cuisines { get; set; }
        public List<MenuItemViewModel> MenuItems { get; set; }

        public int? SelectedCuisineId { get; set; }
    }
}
