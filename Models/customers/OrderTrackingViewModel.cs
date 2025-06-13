namespace Foodie.Models.customers
{
    public class OrderTrackingViewModel
    {
        public UserOrder order { get; set; }
        public List<OrderStatusHistoryViewModel> orderStatusHistory { get; set; }

        public DeliveryPartnerInfo deliveryPartner { get; set; }
        public string GoogleMapsApiKey { get; set; }
    }
}
