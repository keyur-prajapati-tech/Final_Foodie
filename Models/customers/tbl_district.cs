using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodie.Models.customers
{
    public class tbl_district
    {
        [Key]
        public int DistrictId { get; set; }

        [Required]
        public string DisrictName { get; set; }

        [ForeignKey("tbl_state")]
        public int StateId { get; set; }
    }
}
