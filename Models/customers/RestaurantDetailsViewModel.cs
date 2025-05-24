using Foodie.Models.Restaurant;

namespace Foodie.Models.customers
{
    public class RestaurantDetailsViewModel
    {
        public int restaurant_id { get; set; }
        public string restaurant_name { get; set; }
        public List<string> cuisine_name { get; set; }
        public string address { get; set; }
        public string restaurant_contact { get; set; }
        public string open_time { get; set; }
        public string close_time { get; set; }
        public byte[] Restaurant_img { get; set; }
        public byte[] Restaurant_menu_img { get; set; }

        public List<tbl_menu_items> MenuItems { get; set; }
    }
}
