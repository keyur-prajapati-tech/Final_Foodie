using System.ComponentModel.DataAnnotations.Schema;

namespace Foodie.Models.Restaurant
{
    public class tbl_restaurant
    {
        public int restaurant_id { get; set; }
        public string restaurant_name { get; set; }
        public string restaurant_contact { get; set; }
        public string restaurant_email { get; set; }
        public string restaurant_street { get; set; }
        public string restaurant_pincode { get; set; }
        public string restaurant_lat { get; set; }
        public string restaurant_lag { get; set; }
        public bool restaurant_isApprov { get; set; } = false;
        public bool restaurant_isOnline { get; set; } = false;
        public int est_id { get; set; }
        public string est_type { get; set; }
        public string rest_password { get; set; }
        public int owner_id { get; set; } 
        public string owner_name { get; set; }
    }
}
