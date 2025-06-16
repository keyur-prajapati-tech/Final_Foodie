using Foodie.Models.Admin;
using Foodie.Models.Restaurant;

namespace Foodie.Models.customers
{
    public class MenuItemViewModel
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public byte[] MenuImg { get; set; }
        public string MenuDescription { get; set; }
        public decimal Amount { get; set; }
        public int RestaurantId { get; set; }
        public bool IsAvalable { get; set; }


        public int cuisine_id { get; set; }
        public string cuisine_name { get; set; }


        public string MenuImageBase64 { get; set; }
        public bool isVegetarian { get; set; }


        public IEnumerable<tbl_cuisine_master> Cuisine { get; set; }
        public IEnumerable<tbl_menu_items> MenuItems { get; set; }
        public int? SelectedCuisineId { get; set; }
        public string RestaurantName { get; set; }
        public string restaurant_phone { get; set; }
        public string restaurant_email { get; set; }

        //For displaying Top Selling Menu Items
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
