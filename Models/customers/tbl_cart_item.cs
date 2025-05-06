namespace Foodie.Models.customers
{
    public class tbl_cart_item
    {
        public int cart_item_id { get; set; }
        public int cart_id { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public int menu_id { get; set; }
        public int coupone_id { get; set; }
    }
}
