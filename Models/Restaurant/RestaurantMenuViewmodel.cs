using Foodie.Models.Admin;

namespace Foodie.Models.Restaurant
{
    public class RestaurantMenuViewmodel
    {
        public tbl_vendores_img tbl_Vendores_Img { get; set; } = new tbl_vendores_img();

        public tbl_vendor_availability tbl_Vendor_Availability { get; set; } = new tbl_vendor_availability();
        public int restaurant_id { get; set; }
        public string restaurant_name { get; set; }
        public int? SelectedCuisineId { get; set; }
        public List<tbl_cuisine_master> Cuisines { get; set; }
        public List<tbl_menu_items> MenuItems { get; set; }
    }
}
