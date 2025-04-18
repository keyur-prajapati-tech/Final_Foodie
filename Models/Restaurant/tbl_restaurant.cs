﻿using System.ComponentModel.DataAnnotations.Schema;

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
        public bool restaurant_isActive { get; set; } = false;
        public int est_id { get; set; }
        public int owner_id { get; set; } 
    }
}
