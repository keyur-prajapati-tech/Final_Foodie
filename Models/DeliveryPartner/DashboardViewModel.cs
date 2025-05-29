namespace Foodie.Models.DeliveryPartner
{
    public class DashViewModel
    {
        public int OrdersDelivered { get; set; }
        public decimal TotalEarnings { get; set; }
        public double AvgDeliveryTime { get; set; }

        public List<string> WeeklyLabels { get; set; } = new();
        public List<decimal> WeeklyEarnings { get; set; } = new();

        public List<string> OrderStatusLabels { get; set; } = new();
        public List<int> OrderStatusCounts { get; set; } = new();

        // ✅ New properties for map route drawing
        public string RestaurantLat { get; set; } = "";
        public string RestaurantLng { get; set; } = "";
        public string CustomerLat { get; set; } = "";
        public string CustomerLng { get; set; } = "";
    }

}
