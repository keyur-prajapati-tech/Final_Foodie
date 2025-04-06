using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.customers
{
    public class tbl_state
    {
        [Key]
        public int StateId { get; set; }

        [Required]
        public string StateName { get; set; }
    }
}
