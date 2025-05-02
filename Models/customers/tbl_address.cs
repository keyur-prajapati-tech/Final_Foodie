using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.customers
{
    public class tbl_address
    {
        public int address_id { get; set; }
        public int customer_id { get; set; }
        public string addresses_type { get; set; }
        public string CountryName { get; set; }
        public int? StateId { get; set; } // FK to State Table
        public int? DistrictId { get; set; } // FK to District Table
        public int? CityId { get; set; } // FK to City Table
        public string latitude { get; set; } // GPS Coordinates

        public string longitude { get; set; } // GPS Coordinates
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string address { get; set; }
        public string door_no { get; set; }
        public string area { get; set; }
        public string landmark { get; set; }
    }
}
