namespace Foodie.Models.DeliveryPartner
{
    public class AssignedOrderViewModel
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public string FoodStatus { get; set; }
        public DateTime OrderDate { get; set; }

        public string RestaurantName { get; set; }
        public string RestaurantLat { get; set; }
        public string RestaurantLag { get; set; }

        public int CustomerId { get; set; }
        public string AddressType { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int DistrictId { get; set; }

        public string CustomerLatitude { get; set; }
        public string CustomerLongitude { get; set; }

        public double DistanceInKm { get; set; }
        public string EstimatedDeliveryTime { get; set; }

    }


}
