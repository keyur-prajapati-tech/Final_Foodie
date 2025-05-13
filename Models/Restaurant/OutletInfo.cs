namespace Foodie.Models.Restaurant
{
    public class OutletInfo
    {
        public int restaurant_id { get; set; }
        public string restaurant_name { get; set; }
        public string restaurant_street { get; set; }
        public string restaurant_pincode { get; set; }
        public string restaurant_phone { get; set; }
        public string restaurant_email { get; set; }
        public bool restaurant_isOnline { get; set; }

        // Image byte arrays for display
        public byte[] restaurant_img { get; set; }
        public byte[] restaurant_menu_img { get; set; }

        // Files for upload
        public IFormFile NewRestaurantImg { get; set; }
        public IFormFile NewRestaurantMenuImg { get; set; }


    }
}
