namespace Foodie.Models.Restaurant
{
    public class RestaurantViewModel
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string Address { get; set; }
        public decimal Rating { get; set; }
        public string RestaurantImageBase64 { get; set; }
        public string CuisineName { get; set; }
        public string Pincode { get; set; }
        public bool IsOpen { get; set; } // Can be calculated based on business hours
        public double Distance { get; set; } // Can be calculated based on user location
    }
}
