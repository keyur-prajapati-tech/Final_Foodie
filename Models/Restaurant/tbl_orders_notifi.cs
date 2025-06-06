using Foodie.Models.customers;
using Foodie.ViewModels;

namespace Foodie.Models.Restaurant
{
    public class tbl_orders_notifi
    {
        public int order_items_id { get; set; }
        public int order_id { get; set; }
        public int restaurant_id { get; set; }

        public int menu_id { get; set; }
        public int quantity { get; set; }
        public decimal list_price { get; set; }
        public decimal discount { get; set; }

        public DateTime estimated_time { get; set; }
        public string food_status { get; set; }

        public string customer_name { get; set; }

        // Add this:
        public virtual ICollection<OrderItem> Items { get; set; }

    }
}
