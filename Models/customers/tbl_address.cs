using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.customers
{
    public class tbl_address
    {
        [Key]
        public int address_id { get; set; }

        [Required]
        public int customer_id { get; set; }
        public string address_type { get; set; }
        public string CountryName { get; set; }
        public int? StateId { get; set; } // FK to State Table
        public int? DistrictId { get; set; } // FK to District Table
        public int? CityId { get; set; } // FK to City Table
        [StringLength(50)]
        public string latitude { get; set; } // GPS Coordinates

        [StringLength(50)]
        public string longitude { get; set; } // GPS Coordinates
        public string address { get; set; }
    }
}
