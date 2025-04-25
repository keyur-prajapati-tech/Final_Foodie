namespace Foodie.Models.customers
{
    public class CartItemViewModel
    {
        public int cart_item_id { get; set; }
        public int menu_id { get; set; }
        public string menu_name { get; set; }
        public byte[] menu_img { get; set; }
        public decimal amount { get; set; }
        public int quantity { get; set; }
    }
}
