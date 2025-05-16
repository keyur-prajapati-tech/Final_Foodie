using Foodie.Models.Admin;
using Foodie.Models.Restaurant;

namespace Foodie.Models.customers
{
    public class RestaurantMenuViewModel
    {
        public tbl_restaurant Restaurant { get; set; }
        public List<tbl_menu_items> Menu_Items { get; set; }
        public int restauranr_id { get; set; }
        public string restaurant_name { get; set; }

        public List<tbl_cuisine_master> Cuisines { get; set; }
        public List<MenuItemViewModel> MenuItems { get; set; }

        public int? SelectedCuisineId { get; set; }

        public string cuisine_name { get; set; }
        public string restaurant_street { get; set; }
        public string restaurant_pincode { get; set; }
        public string restaurant_contact { get; set; }
        public string open_DATETIME { get; set; }
        public string close_DATETIME { get; set; }
        public byte[] Restaurant_images { get; set; }
        public byte[] Menu_Image { get; set; }
    }
}
