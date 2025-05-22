namespace Foodie.Models.customers
{
    public class OrderItemModel
    {
        public int menu_id { get; set; }
        public int quantity { get; set; }
        public decimal listprice { get; set; }
        public decimal discount { get; set; }
        public DateTime EstimatedTime { get; set; }
    }
}
