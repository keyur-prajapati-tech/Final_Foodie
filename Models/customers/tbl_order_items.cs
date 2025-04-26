namespace Foodie.Models.customers
{
    public class tbl_order_items
    {
        public int order_item_id { get; set; }
        public int order_id { get; set; }
        public int menu_id { get; set; }
        public int quantity { get; set; }
        public decimal list_price { get; set; }
        public decimal discount { get; set; }
        public DateTime estimated_time { get; set; }
    }
}
