using Foodie.Models.Admin;
using Foodie.Models.Restaurant;

namespace Foodie.Models.customers
{
    public class RestaurantMenuModel
    {
        public int RestaurantId { get; set; }
        public int? SelectedCuisineId { get; set; }
        public IEnumerable<tbl_cuisine_master> Cuisines { get; set; }
        public IEnumerable<tbl_menu_items> MenuItems { get; set; }
    }
}
