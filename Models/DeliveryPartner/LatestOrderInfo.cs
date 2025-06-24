namespace Foodie.Models.DeliveryPartner
{
    public class LatestOrderInfo
    {
        public string OrderDate { get; set; }
        public string RestaurantName { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public int ItemCount { get; set; }
        public int OrderCount { get; set; }
    }
}
