using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.customers
{
    public class tbl_city
    {
        [Key]
        public int CityId { get; set; }

        [Required]
        public string CityName { get; set; }

        [ForeignKey("tbl_district")]
        public int DistrictId { get; set; }
    }
}
