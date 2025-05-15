namespace Foodie.Models.customers
{
    public class RestaurantCardViewModel
    {
        public int restaurant_id { get; set; }
        public string restaurant_name { get; set; }
        public string full_address { get; set; }
        public byte[] restaurantImage { get; set; }
    }
}
