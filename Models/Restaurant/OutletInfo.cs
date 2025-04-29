namespace Foodie.Models.Restaurant
{
    public class OutletInfo
    {
        public int restaurant_id { get; set; }
        public string restaurant_name { get; set; }
        public string restaurant_address { get; set; }
        public string restaurant_pincode { get; set; }
        public string restaurant_phone { get; set; }
        public string restaurant_email { get; set; }
      
        public string restaurant_opening_hours { get; set; }
        public string restaurant_closing_hours { get; set; }

        public bool restaurant_isOnline { get; set; }


    }
}
