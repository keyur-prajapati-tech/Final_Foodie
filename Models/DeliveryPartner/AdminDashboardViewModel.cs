namespace Foodie.Models.DeliveryPartner
{
    public class AdminDashboardViewModel
    {
        public int TotalPartners { get; set; }
        public int TotalRestaurants { get; set; }
        public int TotalCustomers { get; set; }

        public List<EarningInfo> RestaurantEarnings { get; set; }
        public List<EarningInfo> PartnerEarnings { get; set; }
    }

    public class EarningInfo
    {
        public string Name { get; set; }
        public decimal TotalEarnings { get; set; }
    }

}
