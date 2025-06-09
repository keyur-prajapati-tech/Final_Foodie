using Foodie.Models.Restaurant;

namespace Foodie.Models.customers
{
    public class tbl_order_items
    {
        //order items table
        public int order_item_id { get; set; }
        public int order_id { get; set; }
        public int menu_id { get; set; }
        public int quantity { get; set; }
        public decimal list_price { get; set; }
        public decimal discount { get; set; }
        public DateTime estimated_time { get; set; }

        public int restaurant_id { get; set; }

        public string? menu_name { get; set; }
        public byte[]? menu_img { get; set; }

        public List<tbl_orders> orders { get; set; } = new List<tbl_orders>();
        public List<tbl_restaurant> restaurants { get; set; } = new List<tbl_restaurant>();
        public List<tbl_menu_items> MenuItems { get; set; } = new List<tbl_menu_items>();
    }
}
