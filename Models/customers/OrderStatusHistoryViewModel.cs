namespace Foodie.Models.customers
{
    public class OrderStatusHistoryViewModel
    {
        public string order_status { get; set; }
        public DateTime order_date { get; set; }

        public DateTime? delivered_date { get; set; }

        public List<tbl_order_items> tbl_Order_Items { get; set; }
        public List<MenuItemViewModel> MenuItems { get; set; }
    }
}
